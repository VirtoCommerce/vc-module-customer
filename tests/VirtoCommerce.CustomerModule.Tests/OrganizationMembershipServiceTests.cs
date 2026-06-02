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
    public async Task GetByIdAsync_EmptyId_ReturnsNull()
    {
        var result = await GetService().GetByIdAsync(string.Empty);
        Assert.Null(result);
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

    private static OrganizationMembershipEntity BuildEntity(string id, bool isLocked = false, DateTime? lockoutEnd = null) =>
        new()
        {
            Id = id,
            UserId = "user1",
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
