using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests
{
    public class MemberResolverTests
    {
        private const string UserId = "user-1";
        private const string OtherUserId = "user-2";

        [Fact]
        public async Task ResolveMemberByIdAsync_SameUserIdWithRequestCache_ResolvesUnderlyingOnlyOnce()
        {
            //Arrange
            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), null, null)).ReturnsAsync((Member)null);

            var factoryCallCount = 0;
            Func<UserManager<ApplicationUser>> userManagerFactory = () =>
            {
                factoryCallCount++;
                return CreateUserManager();
            };

            var requestCacheAccessorMock = CreateRequestCacheAccessorWithCache();

            var resolver = new MemberResolver(memberServiceMock.Object, userManagerFactory, requestCacheAccessorMock.Object);

            //Act
            await resolver.ResolveMemberByIdAsync(UserId);
            await resolver.ResolveMemberByIdAsync(UserId);
            await resolver.ResolveMemberByIdAsync(UserId);

            //Assert
            Assert.Equal(1, factoryCallCount);
        }

        [Fact]
        public async Task ResolveMemberByIdAsync_NoRequestScope_ResolvesUnderlyingEveryCall()
        {
            //Arrange
            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), null, null)).ReturnsAsync((Member)null);

            var factoryCallCount = 0;
            Func<UserManager<ApplicationUser>> userManagerFactory = () =>
            {
                factoryCallCount++;
                return CreateUserManager();
            };

            var requestCacheAccessorMock = new Mock<IRequestScopedCacheAccessor>();
            requestCacheAccessorMock.Setup(x => x.Cache).Returns((IRequestScopedCache)null);

            var resolver = new MemberResolver(memberServiceMock.Object, userManagerFactory, requestCacheAccessorMock.Object);

            //Act
            await resolver.ResolveMemberByIdAsync(UserId);
            await resolver.ResolveMemberByIdAsync(UserId);
            await resolver.ResolveMemberByIdAsync(UserId);

            //Assert
            Assert.Equal(3, factoryCallCount);
        }

        [Fact]
        public async Task ResolveMemberByIdAsync_ObsoleteConstructor_ResolvesUncached()
        {
            //Arrange
            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), null, null)).ReturnsAsync((Member)null);

            var factoryCallCount = 0;
            Func<UserManager<ApplicationUser>> userManagerFactory = () =>
            {
                factoryCallCount++;
                return CreateUserManager();
            };

            // The [Obsolete] ctor supplies a null accessor, so the null-guard in ResolveMemberByIdAsync is the
            // only thing keeping this path from throwing. Nothing else exercises it.
#pragma warning disable VC0012
            var resolver = new MemberResolver(memberServiceMock.Object, userManagerFactory, new Mock<IPlatformMemoryCache>().Object);
#pragma warning restore VC0012

            //Act
            await resolver.ResolveMemberByIdAsync(UserId);
            await resolver.ResolveMemberByIdAsync(UserId);

            //Assert
            Assert.Equal(2, factoryCallCount);
        }

        [Fact]
        public async Task ResolveMemberByIdAsync_DistinctUserIds_CachedIndependently()
        {
            //Arrange
            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), null, null)).ReturnsAsync((Member)null);

            var factoryCallCount = 0;
            Func<UserManager<ApplicationUser>> userManagerFactory = () =>
            {
                factoryCallCount++;
                return CreateUserManager();
            };

            var requestCacheAccessorMock = CreateRequestCacheAccessorWithCache();

            var resolver = new MemberResolver(memberServiceMock.Object, userManagerFactory, requestCacheAccessorMock.Object);

            //Act
            await resolver.ResolveMemberByIdAsync(UserId);
            await resolver.ResolveMemberByIdAsync(UserId);
            await resolver.ResolveMemberByIdAsync(OtherUserId);
            await resolver.ResolveMemberByIdAsync(OtherUserId);

            //Assert
            Assert.Equal(2, factoryCallCount);
        }

        [Fact]
        public void ServiceCollection_ResolvesIMemberResolver_MirroringModuleRegistration()
        {
            //Arrange
            var memberServiceMock = new Mock<IMemberService>();

            var services = new ServiceCollection();
            services.AddSingleton(memberServiceMock.Object);
            services.AddSingleton<Func<UserManager<ApplicationUser>>>(() => CreateUserManager());
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRequestScopedCacheAccessor, HttpRequestScopedCacheAccessor>();

            // Mirrors the explicit factory registration in Module.cs, which pins the exact constructor
            // to avoid ambiguity with the [Obsolete] IPlatformMemoryCache-accepting constructor.
            services.AddTransient<IMemberResolver>(provider => new MemberResolver(
                provider.GetRequiredService<IMemberService>(),
                provider.GetRequiredService<Func<UserManager<ApplicationUser>>>(),
                provider.GetRequiredService<IRequestScopedCacheAccessor>()));

            using var provider = services.BuildServiceProvider(validateScopes: true);

            //Act
            var resolver = provider.GetRequiredService<IMemberResolver>();

            //Assert
            Assert.IsType<MemberResolver>(resolver);
        }

        [Fact]
        public void ServiceCollection_GenericRegistration_ThrowsAmbiguousConstructor()
        {
            //Arrange
            var memberServiceMock = new Mock<IMemberService>();
            var platformMemoryCacheMock = new Mock<IPlatformMemoryCache>();

            var services = new ServiceCollection();
            services.AddSingleton(memberServiceMock.Object);
            services.AddSingleton<Func<UserManager<ApplicationUser>>>(() => CreateUserManager());
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRequestScopedCacheAccessor, HttpRequestScopedCacheAccessor>();
            services.AddSingleton(platformMemoryCacheMock.Object);

            // Pins the premise for the factory registration in Module.cs: with both ctors DI-resolvable,
            // the generic AddTransient<TService, TImplementation>() overload cannot pick a constructor.
            services.AddTransient<IMemberResolver, MemberResolver>();

            using var provider = services.BuildServiceProvider(validateScopes: true);

            //Act
            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IMemberResolver>());

            //Assert
            Assert.Contains("constructor", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ResolveMemberByIdAsync_MemberServiceReturnsMember_ReturnsSameMemberAndCachesIt()
        {
            //Arrange
            var member = new Contact { Id = "member-1" };

            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock.Setup(x => x.GetByIdAsync(UserId, null, null)).ReturnsAsync(member);

            var factoryCallCount = 0;
            Func<UserManager<ApplicationUser>> userManagerFactory = () =>
            {
                factoryCallCount++;
                return CreateUserManager();
            };

            var requestCacheAccessorMock = CreateRequestCacheAccessorWithCache();

            var resolver = new MemberResolver(memberServiceMock.Object, userManagerFactory, requestCacheAccessorMock.Object);

            //Act
            var first = await resolver.ResolveMemberByIdAsync(UserId);
            var second = await resolver.ResolveMemberByIdAsync(UserId);

            //Assert
            Assert.Same(member, first);
            Assert.Same(member, second);
            Assert.Equal(1, factoryCallCount);
        }

        [Fact]
        public async Task ResolveMemberByIdAsync_ConcurrentSameUserId_ResolvesOnceAndSharesResult()
        {
            //Arrange
            var member = new Contact { Id = "member-1" };

            var getByIdCallCount = 0;
            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock.Setup(x => x.GetByIdAsync(UserId, null, null))
                .Returns(async () =>
                {
                    Interlocked.Increment(ref getByIdCallCount);
                    // Yield so concurrent same-key misses are actually exercised.
                    await Task.Yield();
                    return member;
                });

            var requestCacheAccessorMock = CreateRequestCacheAccessorWithCache();

            var resolver = new MemberResolver(memberServiceMock.Object, CreateUserManager, requestCacheAccessorMock.Object);

            //Act
            var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() => resolver.ResolveMemberByIdAsync(UserId)));
            var results = await Task.WhenAll(tasks);

            //Assert
            Assert.Equal(1, getByIdCallCount);
            Assert.All(results, x => Assert.Same(member, x));
        }

        // Real platform cache instance (a plain object - nothing to dispose) behind a stub accessor.
        // Depending on the accessor rather than IHttpContextAccessor is what lets this be a one-liner
        // instead of a fabricated HttpContext wrapping a mocked IServiceProvider.
        private static Mock<IRequestScopedCacheAccessor> CreateRequestCacheAccessorWithCache()
        {
            var accessorMock = new Mock<IRequestScopedCacheAccessor>();
            accessorMock.Setup(x => x.Cache).Returns(new RequestScopedCache());

            return accessorMock;
        }

        private static UserManager<ApplicationUser> CreateUserManager()
        {
            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            storeMock.Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((ApplicationUser)null);

            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            optionsMock.Setup(x => x.Value).Returns(new IdentityOptions());

            return new UserManager<ApplicationUser>(
                storeMock.Object,
                optionsMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }
    }
}
