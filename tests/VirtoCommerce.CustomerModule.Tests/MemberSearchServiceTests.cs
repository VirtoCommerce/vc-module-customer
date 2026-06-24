using System;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests
{
    public class MemberSearchServiceTests
    {
        private const string OrgAId = "org-a";
        private const string OrgBId = "org-b";

        private static readonly string[] RootIds = { OrgAId, OrgBId };
        private static readonly string[] ChildIds = { "child-a", "child-b" };
        private static readonly string[] ChildAIds = { "child-a" };
        private static readonly string[] AllIds = { "child-a", "child-b", OrgAId, OrgBId };

        private readonly Mock<IMemberRepository> _repositoryMock = new();

        public MemberSearchServiceTests()
        {
            // org-a and org-b are roots (no membership relations);
            // child-a -> org-a, child-b -> org-b
            var members = new MemberEntity[]
            {
                Root(OrgAId),
                Root(OrgBId),
                Child("child-a", OrgAId),
                Child("child-b", OrgBId),
            };

            _repositoryMock.SetupGet(x => x.Members).Returns(members.AsQueryable());
        }

        [Fact]
        public void BuildQuery_NoMemberSpecified_ReturnsRootMembersOnly()
        {
            var ids = Search(new MembersSearchCriteria());

            Assert.Equal(RootIds, ids.OrderBy(x => x));
        }

        [Fact]
        public void BuildQuery_DeepSearch_ReturnsAllMembers()
        {
            var ids = Search(new MembersSearchCriteria { DeepSearch = true });

            Assert.Equal(AllIds, ids.OrderBy(x => x));
        }

        [Fact]
        public void BuildQuery_SingleMemberId_ReturnsChildrenOfThatMember()
        {
            var ids = Search(new MembersSearchCriteria { MemberId = OrgAId });

            Assert.Equal(ChildAIds, ids);
        }

        [Fact]
        public void BuildQuery_MultipleMemberIds_ReturnsChildrenOfAnyOfThem()
        {
            var ids = Search(new MembersSearchCriteria { MemberIds = RootIds });

            Assert.Equal(ChildIds, ids.OrderBy(x => x));
        }

        [Fact]
        public void BuildQuery_RootMembersOnlyTrue_AppliesEvenWithDeepSearch()
        {
            // RootMembersOnly is decoupled from DeepSearch: explicitly true wins over a deep search.
            var ids = Search(new MembersSearchCriteria { RootMembersOnly = true, DeepSearch = true });

            Assert.Equal(RootIds, ids.OrderBy(x => x));
        }

        [Fact]
        public void BuildQuery_RootMembersOnlyFalse_ReturnsAllMembersWithoutDeepSearch()
        {
            // RootMembersOnly is decoupled from MemberId: explicitly false disables the legacy root-only default.
            var ids = Search(new MembersSearchCriteria { RootMembersOnly = false });

            Assert.Equal(AllIds, ids.OrderBy(x => x));
        }

        private string[] Search(MembersSearchCriteria criteria)
        {
            var service = new TestableMemberSearchService();
            return service.BuildQuery(_repositoryMock.Object, criteria).Select(x => x.Id).ToArray();
        }

        private static OrganizationEntity Root(string id) =>
            new() { Id = id, MemberType = nameof(Organization) };

        private static ContactEntity Child(string id, string parentId) =>
            new()
            {
                Id = id,
                MemberType = nameof(Contact),
                MemberRelations = new ObservableCollection<MemberRelationEntity>
                {
                    new MemberRelationEntity { AncestorId = parentId, RelationType = RelationType.Membership.ToString() },
                },
            };

        private sealed class TestableMemberSearchService : MemberSearchService
        {
            public TestableMemberSearchService()
                : base(null, null, null, null)
            {
            }

            public new IQueryable<MemberEntity> BuildQuery(IMemberRepository repository, MembersSearchCriteria criteria) =>
                base.BuildQuery(repository, criteria);
        }
    }
}
