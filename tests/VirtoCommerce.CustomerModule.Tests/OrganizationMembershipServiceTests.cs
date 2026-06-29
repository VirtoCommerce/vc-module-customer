using System;
using System.Collections.Generic;
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
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationMembershipServiceTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();

    public OrganizationMembershipServiceTests()
    {
        _repositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(Enumerable.Empty<OrganizationMembershipEntity>().AsTestAsyncQueryable());
        _repositoryMock.Setup(r => r.Organizations)
            .Returns(Enumerable.Empty<OrganizationEntity>().AsTestAsyncQueryable());
    }

    [Fact]
    public async Task GetAsync_EmptyIds_ReturnsEmptyList()
    {
        var result = await GetService().GetAsync([]);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByUserAndOrgAsync_EmptyUserId_ReturnsNull()
    {
        var result = await GetService().GetByUserAndOrgAsync(string.Empty, "org1");
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveChangesAsync_EmptyList_DoesNotCallRepository()
    {
        await GetService().SaveChangesAsync([]);
        _repositoryMock.Verify(r => r.Add(It.IsAny<OrganizationMembershipEntity>()), Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_DuplicateMembership_ThrowsInvalidOperation()
    {
        //Arrange
        var existing = new OrganizationMembershipEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "user1",
            OrganizationId = "org1",
            Roles = new NullCollection<OrganizationMembershipRoleEntity>()
        };
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(new[] { existing }.AsTestAsyncQueryable());

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GetService().SaveChangesAsync([new OrganizationMembership { UserId = "user1", OrganizationId = "org1" }]));
    }

    [Fact]
    public async Task LockAsync_ExistingMembership_SetsLockedState()
    {
        //Arrange
        var lockoutEnd = DateTime.UtcNow.AddDays(7);
        var entity = BuildEntity("id1");
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(new[] { entity }.AsTestAsyncQueryable());

        //Act
        await GetService().LockAsync("id1", lockoutEnd);

        //Assert
        Assert.True(entity.IsLocked);
        Assert.Equal(lockoutEnd, entity.LockoutEnd);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UnlockAsync_ExistingMembership_ClearsLockedState()
    {
        //Arrange
        var entity = BuildEntity("id1", isLocked: true, lockoutEnd: DateTime.UtcNow.AddDays(7));
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(new[] { entity }.AsTestAsyncQueryable());

        //Act
        await GetService().UnlockAsync("id1");

        //Assert
        Assert.False(entity.IsLocked);
        Assert.Null(entity.LockoutEnd);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CountByUserIdAsync_EmptyUserId_ReturnsZero()
    {
        var result = await GetService().CountByUserIdAsync(string.Empty);
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CountByUserIdAsync_WithEntities_ReturnsCorrectCount()
    {
        //Arrange
        var entities = new[]
        {
            BuildEntity("id1", userId: "user1"),
            BuildEntity("id2", userId: "user1"),
            BuildEntity("id3", userId: "other"),
        };
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(entities.AsTestAsyncQueryable());

        //Act
        var result = await GetService().CountByUserIdAsync("user1");

        //Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetLockedOrganizationIdsAsync_EmptyUserId_ReturnsEmpty()
    {
        var result = await GetService().GetLockedOrganizationIdsAsync(string.Empty);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLockedOrganizationIdsAsync_MixedLockStates_ReturnsOnlyActive()
    {
        //Arrange
        var entities = new[]
        {
            BuildEntity("id1", userId: "user1", orgId: "org1", isLocked: true,  lockoutEnd: null),                      // locked indefinitely → included
            BuildEntity("id2", userId: "user1", orgId: "org2", isLocked: true,  lockoutEnd: DateTime.UtcNow.AddDays(1)), // locked future → included
            BuildEntity("id3", userId: "user1", orgId: "org3", isLocked: true,  lockoutEnd: DateTime.UtcNow.AddDays(-1)),// locked expired → excluded
            BuildEntity("id4", userId: "user1", orgId: "org4", isLocked: false, lockoutEnd: null),                       // not locked → excluded
        };
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(entities.AsTestAsyncQueryable());

        //Act
        var result = await GetService().GetLockedOrganizationIdsAsync("user1");

        //Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("org1", result);
        Assert.Contains("org2", result);
    }

    [Fact]
    public async Task SearchAsync_EmptyUserId_ReturnsEmptyResult()
    {
        var result = await GetService().SearchAsync(new OrganizationMembershipSearchCriteria { UserId = string.Empty });
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        //Arrange
        var entities = Enumerable.Range(1, 5)
            .Select(i => BuildEntity($"id{i}", userId: "user1", orgId: $"org{i}"))
            .ToArray();
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(entities.AsTestAsyncQueryable());

        //Act
        var result = await GetService().SearchAsync(new OrganizationMembershipSearchCriteria { UserId = "user1", Skip = 0, Take = 3 });

        //Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    public async Task DeleteAsync_EmptyList_DoesNotCallRepository()
    {
        await GetService().DeleteAsync([]);
        _repositoryMock.Verify(r => r.Remove(It.IsAny<OrganizationMembershipEntity>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithIds_RemovesEntitiesAndCommits()
    {
        //Arrange
        var entity = BuildEntity("id1", userId: "user1");
        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(new[] { entity }.AsTestAsyncQueryable());

        //Act
        await GetService().DeleteAsync(["id1"]);

        //Assert
        _repositoryMock.Verify(r => r.Remove(entity), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_EmptyOrgId_ReturnsEmpty()
    {
        var result = await GetService().GetUserIdsByRoleInOrgAsync(string.Empty, ["role1"]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_EmptyRoleIds_ReturnsEmpty()
    {
        var result = await GetService().GetUserIdsByRoleInOrgAsync("org1", []);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_ReturnsUsersWithMatchingRole()
    {
        //Arrange
        var entities = new[]
        {
            BuildEntityWithRoles("id1", userId: "user1", orgId: "org1", roleId: "admin"),
            BuildEntityWithRoles("id2", userId: "user2", orgId: "org1", roleId: "editor"),
            BuildEntityWithRoles("id3", userId: "user3", orgId: "org1", roleId: "viewer"),
        };

        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(entities.AsTestAsyncQueryable());

        //Act
        var result = await GetService().GetUserIdsByRoleInOrgAsync("org1", ["admin", "editor"]);

        //Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("user1", result);
        Assert.Contains("user2", result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_ExcludesUsersFromDifferentOrg()
    {
        //Arrange
        var entities = new[]
        {
            BuildEntityWithRoles("id1", userId: "user1", orgId: "org1", roleId: "admin"),
            BuildEntityWithRoles("id2", userId: "user2", orgId: "org2", roleId: "admin"),
        };

        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(entities.AsTestAsyncQueryable());

        //Act
        var result = await GetService().GetUserIdsByRoleInOrgAsync("org1", ["admin"]);

        //Assert
        Assert.Single(result);
        Assert.Contains("user1", result);
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_DeduplicatesUserIds()
    {
        // A user with two matching roles should appear only once.
        //Arrange
        var entity = new OrganizationMembershipEntity
        {
            Id = "id1",
            UserId = "user1",
            OrganizationId = "org1",
            IsLocked = false,
            Roles =
            [
                new() { Id = "mr1", RoleId = "admin", RoleName = "Admin" },
                new() { Id = "mr2", RoleId = "editor", RoleName = "Editor" },
            ]
        };

        _repositoryMock.Setup(r => r.OrganizationMemberships)
            .Returns(new[] { entity }.AsTestAsyncQueryable());

        //Act
        var result = await GetService().GetUserIdsByRoleInOrgAsync("org1", ["admin", "editor"]);

        //Assert
        Assert.Single(result);
        Assert.Equal("user1", result.First());
    }

    [Fact]
    public async Task GetUserIdsByRoleInOrgAsync_ReturnsReadOnlyCollection()
    {
        var result = await GetService().GetUserIdsByRoleInOrgAsync(string.Empty, ["role1"]);

        Assert.IsType<IReadOnlyCollection<string>>(result, exactMatch: false);
    }

    private static OrganizationMembershipEntity BuildEntityWithRoles(
        string id,
        string userId = "user1",
        string orgId = "org1",
        string roleId = "role1") =>
        new()
        {
            Id = id,
            UserId = userId,
            OrganizationId = orgId,
            IsLocked = false,
            Roles =
            [
                new() { Id = $"{id}-mr", RoleId = roleId, RoleName = roleId }
            ]
        };

    private static OrganizationMembershipEntity BuildEntity(
        string id,
        string userId = "user1",
        string orgId = "org1",
        bool isLocked = false,
        DateTime? lockoutEnd = null) =>
        new()
        {
            Id = id,
            UserId = userId,
            OrganizationId = orgId,
            IsLocked = isLocked,
            LockoutEnd = lockoutEnd,
            Roles = new NullCollection<OrganizationMembershipRoleEntity>()
        };

    private OrganizationMembershipService GetService()
    {
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var platformMemoryCache = new PlatformMemoryCache(
            memoryCache,
            Options.Create(new CachingOptions()),
            new Mock<ILogger<PlatformMemoryCache>>().Object);

        return new OrganizationMembershipService(
            () => _repositoryMock.Object,
            platformMemoryCache,
            _eventPublisherMock.Object);
    }
}

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
