using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Caching;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CustomerModule.Data.Repositories
{
    public abstract class MemberRepositoryBase : DbContextRepositoryBase<CustomerDbContext>, IMemberRepository
    {
        private static string[] Descendants = new[] { nameof(Contact), nameof(Employee) };
        private static string[] Ancestors = new[] { nameof(Organization) };

        private readonly CustomerDbContext _dbContext;

        protected MemberRepositoryBase(CustomerDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        #region IMemberRepository Members


        public IQueryable<AddressEntity> Addresses => DbContext.Set<AddressEntity>();
        public IQueryable<EmailEntity> Emails => DbContext.Set<EmailEntity>();
        public IQueryable<MemberGroupEntity> Groups => DbContext.Set<MemberGroupEntity>();
        public IQueryable<NoteEntity> Notes => DbContext.Set<NoteEntity>();
        public IQueryable<PhoneEntity> Phones => DbContext.Set<PhoneEntity>();
        public IQueryable<MemberEntity> Members => DbContext.Set<MemberEntity>();
        public IQueryable<MemberRelationEntity> MemberRelations => DbContext.Set<MemberRelationEntity>();
        public IQueryable<SeoInfoEntity> SeoInfos => DbContext.Set<SeoInfoEntity>();
        public IQueryable<MemberDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues => DbContext.Set<MemberDynamicPropertyObjectValueEntity>();

        public virtual async Task<MemberEntity[]> GetMembersByIdsAsync(string[] ids, string responseGroup = null, string[] memberTypes = null)
        {
            if (ids.IsNullOrEmpty())
            {
                return new MemberEntity[] { };
            }

            var result = new List<MemberEntity>();
            //TODO It doesn't work. Maybe these performance changes don't needed anymore.
            // Because EF Core uses TPH inheritance mode and now all these queries should be fast.
            //if (!memberTypes.IsNullOrEmpty())
            //{
            //    foreach (var memberType in memberTypes)
            //    {
            //        //Use special dynamically constructed inner generic method for each passed member type 
            //        //for better query performance
            //        var gm = _genericGetMembersMethodInfo.MakeGenericMethod(Type.GetType(memberType));
            //        var members = gm.Invoke(this, new object[] { ids, responseGroup }) as MemberEntity[];
            //        result.AddRange(members);
            //        //Stop process other types
            //        if (result.Count() == ids.Count())
            //        {
            //            break;
            //        }
            //    }
            //}         
            //else
            {
                var members = await InnerGetMembersByIds<MemberEntity>(ids, responseGroup);
                result.AddRange(members);
            }
            return result.ToArray();
        }

        public virtual async Task RemoveMembersByIdsAsync(string[] ids, string[] memberTypes = null)
        {
            var dbMembers = await GetMembersByIdsAsync(ids, null, memberTypes);
            
            foreach (var dbMember in dbMembers)
            {
                Remove(dbMember);
            }
        }

        public virtual async Task RemoveRelationsByEntitiesAsync(MemberRelationEntity[] relationEntities)
        {
            foreach (var relationEntity in relationEntities)
            {
                Remove(relationEntity);
            }
        }
        #endregion

        public async Task<T[]> InnerGetMembersByIds<T>(string[] ids, string responseGroup = null) where T : MemberEntity
        {
            //Use OfType() clause very much accelerates the query performance when used TPT inheritance
            var query = Members.OfType<T>().Where(x => ids.Contains(x.Id));

            var result = await query.ToArrayAsync();
            ids = result.Select(x => x.Id).ToArray();
            if (!ids.IsNullOrEmpty())
            {
                var relations = await MemberRelations.Where(x => ids.Contains(x.DescendantId)).ToArrayAsync();
                var ancestorIds = relations.Select(x => x.AncestorId).ToArray();
                if (!ancestorIds.IsNullOrEmpty())
                {
                    await Members.Where(x => ancestorIds.Contains(x.Id)).LoadAsync();
                }

                var memberResponseGroup = EnumUtility.SafeParseFlags(responseGroup, MemberResponseGroup.Full);

                if (memberResponseGroup.HasFlag(MemberResponseGroup.WithNotes))
                {
                    await Notes.Where(x => ids.Contains(x.MemberId)).LoadAsync();
                }

                if (memberResponseGroup.HasFlag(MemberResponseGroup.WithEmails))
                {
                    await Emails.Where(x => ids.Contains(x.MemberId)).LoadAsync();
                }

                if (memberResponseGroup.HasFlag(MemberResponseGroup.WithAddresses))
                {
                    await Addresses.Where(x => ids.Contains(x.MemberId)).LoadAsync();
                }

                if (memberResponseGroup.HasFlag(MemberResponseGroup.WithPhones))
                {
                    await Phones.Where(x => ids.Contains(x.MemberId)).LoadAsync();
                }

                if (memberResponseGroup.HasFlag(MemberResponseGroup.WithGroups))
                {
                    await Groups.Where(x => ids.Contains(x.MemberId)).LoadAsync();
                }

                if (memberResponseGroup.HasFlag(MemberResponseGroup.WithSeo))
                {
                    await SeoInfos.Where(x => ids.Contains(x.MemberId)).LoadAsync();
                }

                if (memberResponseGroup.HasFlag(MemberResponseGroup.WithDynamicProperties))
                {
                    await DynamicPropertyObjectValues.Where(x => ids.Contains(x.ObjectId)).LoadAsync();
                }
            }
            return result;
        }

        public async Task<MemberRelationEntity[]> GetRelationsByMembersAsync(IDictionary<string, string> memberIdAndTypeMap)
        {
            var result = Array.Empty<MemberRelationEntity>();

            var descendantIds = memberIdAndTypeMap.Where(x => Descendants.Contains(x.Value)).Select(x => x.Key).Distinct().ToArray();
            var relationsDescendants = await MemberRelations.Where(x => descendantIds.Contains(x.DescendantId)).Include(x => x.Ancestor).ToArrayAsync();
            result = relationsDescendants;
            
            var ancestorIds = memberIdAndTypeMap.Where(x => Ancestors.Contains(x.Value)).Select(x => x.Key).Distinct().ToArray();
            var relationsAncestors = await MemberRelations.Where(x => ancestorIds.Contains(x.AncestorId)).Include(x => x.Descendant).ToArrayAsync();
            result = result.Union(relationsAncestors).ToArray();

            return result;
        }
    }
}
