// Ignore Spelling: Virto

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests
{
    // Any other test class that enqueues through the static BackgroundJob facade must join this collection:
    // the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
    // the static, and a class racing this one would see ObjectDisposedException from it.
    [Collection(nameof(LogChangesMemberChangedEventHandlerTests))]
    [Trait("Category", "CI")]
    public class LogChangesMemberChangedEventHandlerTests
    {
        [Fact]
        public async Task Handle_MemberChangedEvent_EnqueuesOneLogPerChangedEntry()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesEventHandler(Mock.Of<IChangeLogService>());

            var message = new MemberChangedEvent(
            [
                new GenericChangedEntry<Member>(new Contact { Id = "id1" }, new Contact { Id = "id1" }, EntryState.Modified),
                new GenericChangedEntry<Member>(new Contact { Id = "id2" }, new Contact { Id = "id2" }, EntryState.Added),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Equal(2, capture.Payload.OperationLogs.Length);

            // ObjectType is forced to 'Member' (not the concrete Contact/Organization) because
            // MemberDocumentChangesProvider queries all changed members in one request by that type.
            Assert.All(capture.Payload.OperationLogs, x => Assert.Equal(nameof(Member), x.ObjectType));
            Assert.Equal(["id1", "id2"], capture.Payload.OperationLogs.Select(x => x.ObjectId));
            Assert.Equal([EntryState.Modified, EntryState.Added], capture.Payload.OperationLogs.Select(x => x.OperationType));
        }

        [Fact]
        public async Task Handle_UserChangedEvent_SkipsEntriesWithoutMemberId()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesEventHandler(Mock.Of<IChangeLogService>());

            var withMember = new ApplicationUser { Id = "user1", MemberId = "member1" };
            var withoutMember = new ApplicationUser { Id = "user2" };

            var message = new UserChangedEvent(
            [
                new GenericChangedEntry<ApplicationUser>(withMember, withMember, EntryState.Modified),
                new GenericChangedEntry<ApplicationUser>(withoutMember, withoutMember, EntryState.Modified),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            // Count matters as well as contents: one enqueue per changed entry would also leave a
            // single-log Payload behind, since each callback overwrites it.
            Assert.Equal(1, capture.EnqueueCount);

            var log = Assert.Single(capture.Payload.OperationLogs);
            Assert.Equal("member1", log.ObjectId);
            Assert.Equal(nameof(Member), log.ObjectType);
            Assert.Equal(EntryState.Modified, log.OperationType);
        }

        [Fact]
        public async Task Handle_UserRoleAddedEvent_WithMemberId_EnqueuesSingleLog()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesEventHandler(Mock.Of<IChangeLogService>());

            //Act
            await handler.Handle(new UserRoleAddedEvent(new ApplicationUser { Id = "user1", MemberId = "member1" }, "role"));

            //Assert
            var log = Assert.Single(capture.Payload.OperationLogs);
            Assert.Equal("member1", log.ObjectId);
        }

        [Fact]
        public async Task Handle_UserRoleAddedEvent_WithoutMemberId_EnqueuesNothing()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new LogChangesEventHandler(Mock.Of<IChangeLogService>());

            //Act
            await handler.Handle(new UserRoleAddedEvent(new ApplicationUser { Id = "user1" }, "role"));

            //Assert
            Assert.Equal(0, capture.EnqueueCount);
        }

        [Fact]
        public async Task LogEntityChangesInBackground_StillSavesForLegacyHangfireJobs()
        {
            //Arrange
            // Hangfire stores a queued job as type + method name + serialized args, so a store written by an
            // earlier version can still invoke this method. Deleting it would strand those entries as Failed.
            var operationLogs = new[] { AbstractTypeFactory<OperationLog>.TryCreateInstance() };
            var changeLogServiceMock = new Mock<IChangeLogService>();

            var handler = new LogChangesEventHandler(changeLogServiceMock.Object);

            //Act
#pragma warning disable VC0015
            await handler.LogEntityChangesInBackground(operationLogs);
#pragma warning restore VC0015

            //Assert
            changeLogServiceMock.Verify(x => x.SaveChangesAsync(operationLogs), Times.Once);
        }

        [Fact]
        public async Task LogEntityChangesJobHandler_SavesThePayloadLogs()
        {
            //Arrange
            var operationLogs = new[] { AbstractTypeFactory<OperationLog>.TryCreateInstance() };
            var changeLogServiceMock = new Mock<IChangeLogService>();

            var handler = new LogEntityChangesJobHandler(changeLogServiceMock.Object);

            //Act
            await handler.Execute(new LogEntityChangesJobPayload { OperationLogs = operationLogs }, context: null,
                TestContext.Current.CancellationToken);

            //Assert
            changeLogServiceMock.Verify(x => x.SaveChangesAsync(operationLogs), Times.Once);
        }

        // Captures what the handler enqueued through the static BackgroundJob facade. IBackgroundJob is
        // registered Scoped here exactly as the engine module registers it, so this also proves the facade's
        // per-call scope resolves it - the handler itself is root-resolved and must never hold it.
        // The facade is process-global state; xUnit runs one class's tests sequentially and no other test
        // class touches it, so exclusive ownership holds.
        private sealed class EnqueueCapture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public EnqueueCapture()
            {
                BackgroundJobMock
                    .Setup(x => x.Enqueue<LogEntityChangesJobHandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) =>
                    {
                        Payload = (LogEntityChangesJobPayload)payload;
                        EnqueueCount++;
                    })
                    .ReturnsAsync("job-id");

                var services = new ServiceCollection();
                services.AddScoped(_ => BackgroundJobMock.Object);
                _provider = services.BuildServiceProvider(validateScopes: true);

                BackgroundJob.Initialize(_provider);
            }

            public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

            public LogEntityChangesJobPayload Payload { get; private set; }

            public int EnqueueCount { get; private set; }

            public void Dispose()
            {
                _provider.Dispose();
            }
        }
    }
}
