using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Search;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests
{
    public class MemberServiceUnitTests
    {
        private readonly Mock<IMemberRepository> _repositoryMock;
        private readonly Func<IMemberRepository> _repositoryFactory;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<IPlatformMemoryCache> _platformMemoryCacheMock;
        private readonly Mock<IUserSearchService> _userSearchServiceMock;
        private readonly Mock<ICacheEntry> _cacheEntryMock;
        private readonly PrimaryKeyResolvingMap _primaryKeyResolvingMap;

        public MemberServiceUnitTests()
        {
            _repositoryMock = new Mock<IMemberRepository>();
            _repositoryFactory = () => _repositoryMock.Object;
            _eventPublisherMock = new Mock<IEventPublisher>();
            _platformMemoryCacheMock = new Mock<IPlatformMemoryCache>();
            _platformMemoryCacheMock.Setup(x => x.GetDefaultCacheEntryOptions()).Returns(() => new MemoryCacheEntryOptions());

            _userSearchServiceMock = new Mock<IUserSearchService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _repositoryMock.Setup(ss => ss.UnitOfWork).Returns(_mockUnitOfWork.Object);
            _cacheEntryMock = new Mock<ICacheEntry>();
            _cacheEntryMock.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            _primaryKeyResolvingMap = new PrimaryKeyResolvingMap();

            AbstractTypeFactory<Member>.RegisterType<Organization>().MapToType<OrganizationEntity>();
            AbstractTypeFactory<Member>.RegisterType<Contact>().MapToType<ContactEntity>();

            AbstractTypeFactory<MemberEntity>.RegisterType<ContactEntity>();
            AbstractTypeFactory<MemberEntity>.RegisterType<OrganizationEntity>();
        }

        [Fact]
        public async Task DeleteAsync_TryDeleteOrganization_DeleteWithRelations()
        {
            //Arrange
            var service = GetMemberService();
            var organizationEntity = new OrganizationEntity { Id = Guid.NewGuid().ToString(), MemberType = nameof(Organization) };
            var organizationEntity2 = new OrganizationEntity { Id = Guid.NewGuid().ToString(), MemberType = nameof(Organization) };
            var contactEntity = new ContactEntity()
            {
                Id = Guid.NewGuid().ToString(),
                MemberType = nameof(Contact),
                MemberRelations = new ObservableCollection<MemberRelationEntity>
                {
                    new MemberRelationEntity { Ancestor = organizationEntity, AncestorId = organizationEntity.Id, RelationType = RelationType.Membership.ToString() },
                    new MemberRelationEntity { Ancestor = organizationEntity2, AncestorId = organizationEntity2.Id, RelationType = RelationType.Membership.ToString() }
                }
            };

            _repositoryMock.Setup(n => n.GetMembersByIdsAsync(new[] { contactEntity.Id }, null, It.IsAny<string[]>()))
                .ReturnsAsync(new[] { contactEntity });

            _repositoryMock.Setup(n => n.GetMembersByIdsAsync(new[] { organizationEntity.Id }, null, It.IsAny<string[]>()))
                .ReturnsAsync(new[] { organizationEntity });

            var cacheKey = CacheKey.With(service.GetType(), nameof(service.GetByIdsAsync), string.Join("-", new[] { contactEntity.Id }), null, null);
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(_cacheEntryMock.Object);

            var cacheKeyOrg = CacheKey.With(service.GetType(), nameof(service.GetByIdsAsync), string.Join("-", new[] { organizationEntity.Id }), null, null);
            _platformMemoryCacheMock.Setup(pmc => pmc.CreateEntry(cacheKeyOrg)).Returns(_cacheEntryMock.Object);

            _repositoryMock.Setup(x => x.RemoveMembersByIdsAsync(new[] { organizationEntity.Id }, null))
                .Callback(() =>
                {
                    contactEntity.MemberRelations = new ObservableCollection<MemberRelationEntity>
                    {
                        new MemberRelationEntity { Ancestor = organizationEntity2, AncestorId = organizationEntity2.Id, RelationType = RelationType.Membership.ToString() }
                    };
                    _repositoryMock.Setup(n => n.GetMembersByIdsAsync(new[] { contactEntity.Id }, null, It.IsAny<string[]>()))
                        .ReturnsAsync(new[] { contactEntity });
                });

            //Act
            var contact = (Contact)await service.GetByIdAsync(contactEntity.Id);
            await service.DeleteAsync(new[] { organizationEntity.Id });
            var afterDeleteContact = (Contact)await service.GetByIdAsync(contactEntity.Id);

            //Assert
            Assert.Contains(organizationEntity.Id, contact.Organizations);
            Assert.DoesNotContain(organizationEntity.Id, afterDeleteContact.Organizations);
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveContact_ReturnCachedContact()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newContact = new Contact
            {
                Id = id,
                Name = "some contact",
            };
            var newContactEntity = AbstractTypeFactory<ContactEntity>.TryCreateInstance().FromModel(newContact, new PrimaryKeyResolvingMap());
            var service = GetMemberServiceWithPlatformMemoryCache();
            _repositoryMock.Setup(x => x.Add(newContactEntity))
                .Callback(() =>
                {
                    _repositoryMock.Setup(o => o.GetMembersByIdsAsync(new[] { id }, null, It.IsAny<string[]>()))
                        .ReturnsAsync(new[] { newContactEntity });
                });

            //Act
            var nullContact = await service.GetByIdAsync(id, null);
            await service.SaveChangesAsync(new[] { newContact });
            var contact = await service.GetByIdAsync(id, null);

            //Assert
            Assert.NotEqual(nullContact, contact);
        }

        [Fact]
        public async Task SaveChanges_SaveExistingContact_ModifiedDateIsSet()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var contact = new Contact
            {
                Id = id,
            };

            var originalEntity = new ContactEntity
            {
                Id = id,
            };

            _repositoryMock.Setup(x => x.GetMembersByIdsAsync(new[] { contact.Id }, It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new[] { originalEntity });
            var service = GetMemberService();

            //Act
            await service.SaveChangesAsync(new[] { contact });

            //Assert
            Assert.NotNull(originalEntity.ModifiedDate);
        }


        private MemberService GetMemberService()
        {
            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(It.IsAny<UserSearchCriteria>()))
                .ReturnsAsync(new UserSearchResult());

            return GetMemberService(_platformMemoryCacheMock.Object);
        }

        private MemberService GetMemberServiceWithPlatformMemoryCache()
        {
            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(It.IsAny<UserSearchCriteria>()))
                .ReturnsAsync(new UserSearchResult());

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            return GetMemberService(platformMemoryCache);
        }

        private MemberService GetMemberService(IPlatformMemoryCache platformMemoryCache)
        {
            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(It.IsAny<UserSearchCriteria>()))
                .ReturnsAsync(new UserSearchResult());

            return new MemberService(_repositoryFactory, _userSearchServiceMock.Object, _eventPublisherMock.Object, platformMemoryCache);
        }
    }
}
