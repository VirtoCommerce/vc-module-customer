using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public abstract class OrganizationMembershipServiceTestsBase
{
    protected readonly Mock<ICustomerRepository> RepositoryMock = new();
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock = new();
    protected readonly Mock<IEventPublisher> EventPublisherMock = new();
    protected readonly Mock<IMemberService> MemberServiceMock = new();

    protected OrganizationMembershipServiceTestsBase()
    {
        RepositoryMock.Setup(r => r.UnitOfWork).Returns(UnitOfWorkMock.Object);
        RepositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(Enumerable.Empty<OrganizationMembershipEntity>().AsTestAsyncQueryable());
        RepositoryMock.Setup(r => r.Organizations)
            .Returns(Enumerable.Empty<OrganizationEntity>().AsTestAsyncQueryable());
    }

    protected void SetupMemberships(params OrganizationMembershipEntity[] entities) =>
        RepositoryMock.Setup(r => r.OrganizationMemberships).Returns(entities.AsTestAsyncQueryable());

    protected OrganizationMembershipService CreateCrudService() => CreateServices().Crud;

    protected OrganizationMembershipSearchService CreateSearchService() => CreateServices().Search;

    // The CRUD service's (obsolete) search shims delegate to the search service, and the search service
    // depends on the CRUD service to load models — wire both with a shared cache and a lazy back-reference.
    private (OrganizationMembershipService Crud, OrganizationMembershipSearchService Search) CreateServices()
    {
        var platformMemoryCache = CreatePlatformMemoryCache();
        OrganizationMembershipSearchService search = null;

        var crud = new OrganizationMembershipService(
            () => RepositoryMock.Object, platformMemoryCache, EventPublisherMock.Object, () => search, MemberServiceMock.Object);

        search = new OrganizationMembershipSearchService(
            () => RepositoryMock.Object, platformMemoryCache, crud, Options.Create(new CrudOptions()));

        return (crud, search);
    }

    protected static IPlatformMemoryCache CreatePlatformMemoryCache() =>
        new PlatformMemoryCache(
            new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new CachingOptions()),
            new Mock<ILogger<PlatformMemoryCache>>().Object);

    protected static OrganizationMembershipEntity BuildEntity(
        string id,
        string userId = "user1",
        string orgId = "org1",
        bool isLocked = false,
        DateTime? lockoutEnd = null,
        params string[] roleIds) =>
        new()
        {
            Id = id,
            UserId = userId,
            OrganizationId = orgId,
            IsLocked = isLocked,
            LockoutEnd = lockoutEnd,
            Roles = roleIds is { Length: > 0 }
                ? new ObservableCollection<OrganizationMembershipRoleEntity>(
                    roleIds.Select(rid => new OrganizationMembershipRoleEntity { RoleId = rid, MembershipId = id }))
                : new NullCollection<OrganizationMembershipRoleEntity>(),
        };
}

public class OrganizationMembershipServiceTests : OrganizationMembershipServiceTestsBase
{
    [Fact]
    public async Task GetAsync_EmptyIds_ReturnsEmptyList()
    {
        var result = await CreateCrudService().GetAsync([]);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveChangesAsync_EmptyList_DoesNotCallRepository()
    {
        await CreateCrudService().SaveChangesAsync([]);
        RepositoryMock.Verify(r => r.Add(It.IsAny<OrganizationMembershipEntity>()), Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateMembership_ThrowsInvalidOperation()
    {
        //Arrange
        SetupMemberships(BuildEntity(Guid.NewGuid().ToString(), userId: "user1", orgId: "org1"));

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateCrudService().SaveChangesAsync([new OrganizationMembership { UserId = "user1", OrganizationId = "org1" }]));
    }

    [Fact]
    public async Task LockAsync_ExistingMembership_SetsLockedState()
    {
        //Arrange
        var lockoutEnd = DateTime.UtcNow.AddDays(7);
        var entity = BuildEntity("id1");
        SetupMemberships(entity);

        //Act
        await CreateCrudService().LockAsync("id1", lockoutEnd);

        //Assert
        Assert.True(entity.IsLocked);
        Assert.Equal(lockoutEnd, entity.LockoutEnd);
        UnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UnlockAsync_ExistingMembership_ClearsLockedState()
    {
        //Arrange
        var entity = BuildEntity("id1", isLocked: true, lockoutEnd: DateTime.UtcNow.AddDays(7));
        SetupMemberships(entity);

        //Act
        await CreateCrudService().UnlockAsync("id1");

        //Assert
        Assert.False(entity.IsLocked);
        Assert.Null(entity.LockoutEnd);
        UnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_EmptyList_DoesNotCallRepository()
    {
        await CreateCrudService().DeleteAsync([]);
        RepositoryMock.Verify(r => r.Remove(It.IsAny<OrganizationMembershipEntity>()), Times.Never);
        UnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithIds_RemovesEntitiesAndCommits()
    {
        //Arrange
        var entity = BuildEntity("id1", userId: "user1");
        SetupMemberships(entity);

        //Act
        await CreateCrudService().DeleteAsync(["id1"]);

        //Assert
        RepositoryMock.Verify(r => r.Remove(entity), Times.Once);
        UnitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_EmptyOrgId_ReturnsEmpty()
    {
        var result = await CreateCrudService().GetUserIdsByRoleInOrgAsync(string.Empty, ["role1"]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_EmptyRoleIds_ReturnsEmpty()
    {
        var result = await CreateCrudService().GetUserIdsByRoleInOrgAsync("org1", []);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_ReturnsUsersWithMatchingRole()
    {
        //Arrange
        SetupMemberships(
            BuildEntity("id1", userId: "user1", orgId: "org1", roleIds: "admin"),
            BuildEntity("id2", userId: "user2", orgId: "org1", roleIds: "editor"),
            BuildEntity("id3", userId: "user3", orgId: "org1", roleIds: "viewer"));

        //Act
        var result = await CreateCrudService().GetUserIdsByRoleInOrgAsync("org1", ["admin", "editor"]);

        //Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("user1", result);
        Assert.Contains("user2", result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_ExcludesUsersFromDifferentOrg()
    {
        //Arrange
        SetupMemberships(
            BuildEntity("id1", userId: "user1", orgId: "org1", roleIds: "admin"),
            BuildEntity("id2", userId: "user2", orgId: "org2", roleIds: "admin"));

        //Act
        var result = await CreateCrudService().GetUserIdsByRoleInOrgAsync("org1", ["admin"]);

        //Assert
        Assert.Single(result);
        Assert.Contains("user1", result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_DeduplicatesUserIds()
    {
        // A user with two matching roles should appear only once.
        //Arrange
        SetupMemberships(BuildEntity("id1", userId: "user1", orgId: "org1", roleIds: ["admin", "editor"]));

        //Act
        var result = await CreateCrudService().GetUserIdsByRoleInOrgAsync("org1", ["admin", "editor"]);

        //Assert
        Assert.Single(result);
        Assert.Equal("user1", result.First());
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_ReturnsReadOnlyCollection()
    {
        var result = await CreateCrudService().GetUserIdsByRoleInOrgAsync(string.Empty, ["role1"]);

        Assert.IsType<IReadOnlyCollection<string>>(result, exactMatch: false);
    }

    [Fact]
    public async Task GetRolesByUserAndOrgAsync_EmptyOrgId_ReturnsEmpty()
    {
        var result = await CreateCrudService().GetRolesByUserAndOrgAsync("user1", string.Empty);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRolesByUserAndOrgAsync_EmptyUserId_ReturnsEmpty()
    {
        var result = await CreateCrudService().GetRolesByUserAndOrgAsync(string.Empty, "org1");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRolesByUserAndOrgAsync_OrgRolesOnly_ReturnsOrgRoles()
    {
        //Arrange — org has a role, user has no membership row
        MemberServiceMock
            .Setup(s => s.GetByIdAsync("org1", It.IsAny<string>(), nameof(Organization)))
            .ReturnsAsync(new Organization
            {
                Id = "org1",
                Roles = [new OrganizationRole { RoleId = "r1", RoleName = "Admin" }]
            });

        //Act
        var result = await CreateCrudService().GetRolesByUserAndOrgAsync("user1", "org1");

        //Assert
        Assert.Single(result);
        Assert.Equal("r1", result.First().RoleId);
    }

    [Fact]
    public async Task GetRolesByUserAndOrgAsync_MembershipRolesOnly_ReturnsMembershipRoles()
    {
        //Arrange — org has no roles, user has membership with a role
        MemberServiceMock
            .Setup(s => s.GetByIdAsync("org1", It.IsAny<string>(), nameof(Organization)))
            .ReturnsAsync(new Organization { Id = "org1", Roles = [] });

        SetupMemberships(BuildEntity("m1", userId: "user1", orgId: "org1", roleIds: "r2"));

        //Act
        var result = await CreateCrudService().GetRolesByUserAndOrgAsync("user1", "org1");

        //Assert
        Assert.Single(result);
        Assert.Equal("r2", result.First().RoleId);
    }

    [Fact]
    public async Task GetRolesByUserAndOrgAsync_BothSources_ReturnsMergedRoles()
    {
        //Arrange — org has role A, membership has role B
        MemberServiceMock
            .Setup(s => s.GetByIdAsync("org1", It.IsAny<string>(), nameof(Organization)))
            .ReturnsAsync(new Organization
            {
                Id = "org1",
                Roles = [new OrganizationRole { RoleId = "r1", RoleName = "Admin" }]
            });

        SetupMemberships(BuildEntity("m1", userId: "user1", orgId: "org1", roleIds: "r2"));

        //Act
        var result = await CreateCrudService().GetRolesByUserAndOrgAsync("user1", "org1");

        //Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.RoleId == "r1");
        Assert.Contains(result, r => r.RoleId == "r2");
    }

    [Fact]
    public async Task GetRolesByUserAndOrgAsync_OverlappingRoles_Deduplicates()
    {
        //Arrange — org and membership both have the same role ID
        MemberServiceMock
            .Setup(s => s.GetByIdAsync("org1", It.IsAny<string>(), nameof(Organization)))
            .ReturnsAsync(new Organization
            {
                Id = "org1",
                Roles = [new OrganizationRole { RoleId = "r1", RoleName = "Admin" }]
            });

        SetupMemberships(BuildEntity("m1", userId: "user1", orgId: "org1", roleIds: "r1"));

        //Act
        var result = await CreateCrudService().GetRolesByUserAndOrgAsync("user1", "org1");

        //Assert — r1 appears exactly once
        Assert.Single(result);
        Assert.Equal("r1", result.First().RoleId);
    }

    [Fact]
    public async Task GetRolesByUserAndOrgAsync_OrgNotFound_ReturnsMembershipRolesOnly()
    {
        //Arrange — memberService returns null (org not found)
        MemberServiceMock
            .Setup(s => s.GetByIdAsync("org1", It.IsAny<string>(), nameof(Organization)))
            .ReturnsAsync((Member)null);

        SetupMemberships(BuildEntity("m1", userId: "user1", orgId: "org1", roleIds: "r2"));

        //Act
        var result = await CreateCrudService().GetRolesByUserAndOrgAsync("user1", "org1");

        //Assert — membership roles still returned
        Assert.Single(result);
        Assert.Equal("r2", result.First().RoleId);
    }
}

public class OrganizationMembershipSearchServiceTests : OrganizationMembershipServiceTestsBase
{
    [Fact]
    public async Task SearchAsync_NoEntities_ReturnsEmptyResult()
    {
        var result = await CreateSearchService().SearchAsync(new OrganizationMembershipSearchCriteria { UserId = "user1" });

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task SearchAsync_ByUserAndOrganization_ReturnsMatchingMembership()
    {
        //Arrange
        SetupMemberships(
            BuildEntity("id1", userId: "user1", orgId: "org1"),
            BuildEntity("id2", userId: "user1", orgId: "org2"),
            BuildEntity("id3", userId: "user2", orgId: "org1"));

        //Act
        var result = await CreateSearchService().SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = "user1",
            OrganizationId = "org1",
            Take = 1,
        });

        //Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("id1", result.Results.Single().Id);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        //Arrange
        SetupMemberships(Enumerable.Range(1, 5)
            .Select(i => BuildEntity($"id{i}", userId: "user1", orgId: $"org{i}"))
            .ToArray());

        //Act
        var result = await CreateSearchService().SearchAsync(
            new OrganizationMembershipSearchCriteria { UserId = "user1", Skip = 0, Take = 3 });

        //Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    public async Task SearchAsync_CountOnly_TakeZero_ReturnsTotalCountWithoutResults()
    {
        //Arrange
        SetupMemberships(
            BuildEntity("id1", userId: "user1"),
            BuildEntity("id2", userId: "user1"),
            BuildEntity("id3", userId: "other"));

        //Act
        var result = await CreateSearchService().SearchAsync(
            new OrganizationMembershipSearchCriteria { UserId = "user1", Take = 0 });

        //Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task SearchAsync_OnlyLocked_ReturnsOnlyCurrentlyLocked()
    {
        //Arrange
        SetupMemberships(
            BuildEntity("id1", userId: "user1", orgId: "org1", isLocked: true, lockoutEnd: null),                       // locked indefinitely → included
            BuildEntity("id2", userId: "user1", orgId: "org2", isLocked: true, lockoutEnd: DateTime.UtcNow.AddDays(1)),  // locked future → included
            BuildEntity("id3", userId: "user1", orgId: "org3", isLocked: true, lockoutEnd: DateTime.UtcNow.AddDays(-1)), // locked expired → excluded
            BuildEntity("id4", userId: "user1", orgId: "org4", isLocked: false, lockoutEnd: null));                      // not locked → excluded

        //Act
        var result = await CreateSearchService().SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = "user1",
            OnlyLocked = true,
            Take = int.MaxValue,
        });

        //Assert
        Assert.Equal(2, result.TotalCount);
        var orgIds = result.Results.Select(m => m.OrganizationId).ToList();
        Assert.Contains("org1", orgIds);
        Assert.Contains("org2", orgIds);
    }

    [Fact]
    public async Task GetCountsByUserAsync_GroupsMatchingMembershipsByUser()
    {
        //Arrange — only memberships carrying role "r1" should be counted, one row per user.
        SetupMemberships(
            BuildEntity("id1", userId: "user1", orgId: "org1", roleIds: ["r1"]),
            BuildEntity("id2", userId: "user1", orgId: "org2", roleIds: ["r1"]),
            BuildEntity("id3", userId: "user2", orgId: "org1", roleIds: ["r1"]),
            BuildEntity("id4", userId: "user3", orgId: "org1", roleIds: ["r2"]));

        //Act
        var counts = await CreateSearchService().GetCountsByUserAsync(
            new OrganizationMembershipSearchCriteria { RoleIds = ["r1"] });

        //Assert
        Assert.Equal(2, counts.Count);
        Assert.Equal(2, counts["user1"]);
        Assert.Equal(1, counts["user2"]);
        Assert.False(counts.ContainsKey("user3"));
    }
}

// Back-compat: the [Obsolete] shims on IOrganizationMembershipService must keep working (delegating to the
// search service) so already-published consumers compiled against the combined interface keep resolving them.
#pragma warning disable VC0015
public class OrganizationMembershipObsoleteShimTests : OrganizationMembershipServiceTestsBase
{
    [Fact]
    public async Task GetByUserAndOrgAsync_ReturnsMatchingMembership()
    {
        SetupMemberships(
            BuildEntity("id1", userId: "user1", orgId: "org1"),
            BuildEntity("id2", userId: "user1", orgId: "org2"));

        var result = await CreateCrudService().GetByUserAndOrgAsync("user1", "org2");

        Assert.NotNull(result);
        Assert.Equal("id2", result.Id);
    }

    [Fact]
    public async Task GetByUserAndOrgAsync_EmptyArguments_ReturnsNull()
    {
        Assert.Null(await CreateCrudService().GetByUserAndOrgAsync(string.Empty, "org1"));
    }

    [Fact]
    public async Task CountByUserIdAsync_ReturnsCount()
    {
        SetupMemberships(
            BuildEntity("id1", userId: "user1"),
            BuildEntity("id2", userId: "user1"),
            BuildEntity("id3", userId: "other"));

        Assert.Equal(2, await CreateCrudService().CountByUserIdAsync("user1"));
    }

    [Fact]
    public async Task GetLockedOrganizationIdsAsync_ReturnsCurrentlyLockedOrgs()
    {
        SetupMemberships(
            BuildEntity("id1", userId: "user1", orgId: "org1", isLocked: true, lockoutEnd: null),
            BuildEntity("id2", userId: "user1", orgId: "org2", isLocked: true, lockoutEnd: DateTime.UtcNow.AddDays(-1)),
            BuildEntity("id3", userId: "user1", orgId: "org3", isLocked: false));

        var result = await CreateCrudService().GetLockedOrganizationIdsAsync("user1");

        Assert.Single(result);
        Assert.Contains("org1", result);
    }

    [Fact]
    public async Task GetOrganizationCountsByUserAsync_GroupsByUser()
    {
        SetupMemberships(
            BuildEntity("id1", userId: "user1", roleIds: ["r1"]),
            BuildEntity("id2", userId: "user1", roleIds: ["r1"]),
            BuildEntity("id3", userId: "user2", roleIds: ["r1"]));

        var counts = await CreateCrudService().GetOrganizationCountsByUserAsync(["r1"]);

        Assert.Equal(2, counts["user1"]);
        Assert.Equal(1, counts["user2"]);
    }

    [Fact]
    public async Task SearchAsync_DelegatesToSearchService()
    {
        SetupMemberships(
            BuildEntity("id1", userId: "user1"),
            BuildEntity("id2", userId: "user1"));

        var result = await CreateCrudService().SearchAsync(new OrganizationMembershipSearchCriteria { UserId = "user1" });

        Assert.Equal(2, result.TotalCount);
    }
}
#pragma warning restore VC0015

internal static class TestAsyncQueryableExtensions
{
    internal static IQueryable<T> AsTestAsyncQueryable<T>(this IEnumerable<T> source) =>
        new TestAsyncEnumerable<T>(source);
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
}

internal class TestAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression) =>
        new TestAsyncEnumerable<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression) => inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken token = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var syncResult = inner.Execute(expression);
        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [syncResult])!;
    }
}
