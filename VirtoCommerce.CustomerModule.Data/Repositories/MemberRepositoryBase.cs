using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.CustomerModule.Data.Repositories
{
    public abstract class MemberRepositoryBase : EFRepositoryBase, IMemberRepository
    {
        private static MethodInfo _genericGetMembersMethodInfo;
        static MemberRepositoryBase()
        {
            _genericGetMembersMethodInfo = typeof(MemberRepositoryBase).GetMethod("InnerGetMembersByIds");
        }

        protected MemberRepositoryBase()
        {
        }

        protected MemberRepositoryBase(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.ProxyCreationEnabled = false;
        }

        protected MemberRepositoryBase(DbConnection existingConnection, IUnitOfWork unitOfWork = null,
            IInterceptor[] interceptors = null) : base(existingConnection, unitOfWork, interceptors)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            #region Member
            modelBuilder.Entity<MemberDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<MemberDataEntity>().ToTable("Member");

            #endregion

            #region MemberRelation
            modelBuilder.Entity<MemberRelationDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<MemberRelationDataEntity>().ToTable("MemberRelation");

            modelBuilder.Entity<MemberRelationDataEntity>().HasRequired(m => m.Descendant)
                                                 .WithMany(m => m.MemberRelations).WillCascadeOnDelete(false);
            #endregion

            #region Address
            modelBuilder.Entity<AddressDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<AddressDataEntity>().ToTable("Address");

            modelBuilder.Entity<AddressDataEntity>().HasRequired(m => m.Member)
                                          .WithMany(m => m.Addresses).HasForeignKey(m => m.MemberId)
                                          .WillCascadeOnDelete(true);
            #endregion

            #region Email
            modelBuilder.Entity<EmailDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<EmailDataEntity>().ToTable("Email");

            modelBuilder.Entity<EmailDataEntity>().HasRequired(m => m.Member)
                                          .WithMany(m => m.Emails).HasForeignKey(m => m.MemberId)
                                          .WillCascadeOnDelete(true);
            #endregion

            #region Group
            modelBuilder.Entity<MemberGroupDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<MemberGroupDataEntity>().ToTable("MemberGroup");

            modelBuilder.Entity<MemberGroupDataEntity>().HasRequired(m => m.Member)
                                          .WithMany(m => m.Groups).HasForeignKey(m => m.MemberId)
                                          .WillCascadeOnDelete(true);
            #endregion

            #region Phone
            modelBuilder.Entity<PhoneDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<PhoneDataEntity>().ToTable("Phone");

            modelBuilder.Entity<PhoneDataEntity>().HasRequired(m => m.Member)
                                          .WithMany(m => m.Phones).HasForeignKey(m => m.MemberId)
                                          .WillCascadeOnDelete(true);
            #endregion

            #region Note
            modelBuilder.Entity<NoteDataEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<NoteDataEntity>().ToTable("Note");

            modelBuilder.Entity<NoteDataEntity>().HasOptional(m => m.Member)
                                          .WithMany(m => m.Notes).HasForeignKey(m => m.MemberId)
                                          .WillCascadeOnDelete(true);
            #endregion

            base.OnModelCreating(modelBuilder);
        }


        #region IMemberRepository Members


        public IQueryable<AddressDataEntity> Addresses
        {
            get { return GetAsQueryable<AddressDataEntity>(); }
        }

        public IQueryable<EmailDataEntity> Emails
        {
            get { return GetAsQueryable<EmailDataEntity>(); }
        }


        public IQueryable<MemberGroupDataEntity> Groups
        {
            get { return GetAsQueryable<MemberGroupDataEntity>(); }
        }

        public IQueryable<NoteDataEntity> Notes
        {
            get { return GetAsQueryable<NoteDataEntity>(); }
        }

        public IQueryable<PhoneDataEntity> Phones
        {
            get { return GetAsQueryable<PhoneDataEntity>(); }
        }


        public IQueryable<MemberDataEntity> Members
        {
            get { return GetAsQueryable<MemberDataEntity>(); }
        }

        public IQueryable<MemberRelationDataEntity> MemberRelations
        {
            get { return GetAsQueryable<MemberRelationDataEntity>(); }
        }

        public virtual MemberDataEntity[] GetMembersByIds(string[] ids, string responseGroup = null, string[] memberTypes = null)
        {
            if (ids.IsNullOrEmpty())
            {
                return new MemberDataEntity[] { };
            }

            var memberResponseGroup = EnumUtility.SafeParseFlags(responseGroup, MemberResponseGroup.Full);

            MemberDataEntity[] GetMembersViaGenericReflectionCall(string[] memberIds, MemberResponseGroup memberRespGroup, Type memberType)
            {
                //Use special dynamically constructed inner generic method for each passed member type 
                //for better query performance
                var gm = _genericGetMembersMethodInfo.MakeGenericMethod(memberType);
                return gm.Invoke(this, new object[] { memberIds, memberRespGroup }) as MemberDataEntity[];
            }

            //There is loading for all corresponding members conceptual model entities types
            //query performance when TPT inheritance used it is too slow, for improve performance we are passing concrete member types in to the repository
            var allKnownMemberMappedTypeInfos = AbstractTypeFactory<Member>.AllTypeInfos.Where(t => t.MappedType != null).ToArray();
            var memberMappedTypes = allKnownMemberMappedTypeInfos.Where(x => memberTypes == null || memberTypes.Any(mt => x.IsAssignableTo(mt)))
                                                     .Select(x => x.MappedType.AssemblyQualifiedName)
                                                     .Distinct()
                                                     .Select(x => Type.GetType(x))
                                                     .ToArray();


            var result = new List<MemberDataEntity>();
            if (!memberMappedTypes.IsNullOrEmpty())
            {
                foreach (var memberType in memberMappedTypes)
                {
                    //Use special dynamically constructed inner generic method for each passed member type 
                    //for better query performance

                    result.AddRange(GetMembersViaGenericReflectionCall(ids, memberResponseGroup, memberType));
                    //Stop process other types
                    if (result.Count >= ids.Count())
                    {
                        break;
                    }
                }
            }
            else
            {
                result.AddRange(InnerGetMembersByIds<MemberDataEntity>(ids, memberResponseGroup));
            }

            if (memberResponseGroup.HasFlag(MemberResponseGroup.WithAncestors))
            {
                var ancestors = new List<MemberDataEntity>();
                var relations = MemberRelations.Where(x => ids.Contains(x.DescendantId)).ToArray();
                var ancestorIds = relations.Select(x => x.AncestorId).ToArray();
                if (!ancestorIds.IsNullOrEmpty())
                {
                    //TODO: Need to load  ancestor (member) type from MemberRelations table to avoid  this iteration  on all known member types in the future.
                    var allKnownAncestorTypes = allKnownMemberMappedTypeInfos.Select(x => x.MappedType.AssemblyQualifiedName)
                                                     .Distinct()
                                                     .Select(x => Type.GetType(x))
                                                     .ToArray();
                    //Iterate all known member types  and try to load ancestors for each of them 
                    foreach (var knownAncestorType in allKnownAncestorTypes)
                    {
                        ancestors.AddRange(GetMembersViaGenericReflectionCall(ancestorIds, MemberResponseGroup.None, knownAncestorType).ToArray());
                        //Stop process other types if already loaded all ancestors
                        if (ancestors.Count >= ancestorIds.Count())
                        {
                            break;
                        }
                    }
                }
            }
            return result.ToArray();
        }

        public virtual void RemoveMembersByIds(string[] ids, string[] memberTypes = null)
        {
            var dbMembers = GetMembersByIds(ids, null, memberTypes);
            foreach (var dbMember in dbMembers)
            {
                foreach (var relation in dbMember.MemberRelations.ToArray())
                {
                    Remove(relation);
                }
                Remove(dbMember);
            }
        }
        #endregion

        public T[] InnerGetMembersByIds<T>(string[] ids, MemberResponseGroup responseGroup = MemberResponseGroup.Full) where T : MemberDataEntity
        {
            //Use OfType() clause very much accelerates the query performance when used TPT inheritance
            var query = Members.OfType<T>().Where(x => ids.Contains(x.Id));

            var retVal = query.ToArray();
            ids = retVal.Select(x => x.Id).ToArray();
            if (!ids.IsNullOrEmpty())
            {
                if (responseGroup.HasFlag(MemberResponseGroup.WithNotes))
                {
                    var notes = Notes.Where(x => ids.Contains(x.MemberId)).ToArray();
                }
                if (responseGroup.HasFlag(MemberResponseGroup.WithEmails))
                {
                    var emails = Emails.Where(x => ids.Contains(x.MemberId)).ToArray();
                }
                if (responseGroup.HasFlag(MemberResponseGroup.WithAddresses))
                {
                    var addresses = Addresses.Where(x => ids.Contains(x.MemberId)).ToArray();
                }
                if (responseGroup.HasFlag(MemberResponseGroup.WithPhones))
                {
                    var phones = Phones.Where(x => ids.Contains(x.MemberId)).ToArray();
                }
                if (responseGroup.HasFlag(MemberResponseGroup.WithGroups))
                {
                    var groups = Groups.Where(x => ids.Contains(x.MemberId)).ToArray();
                }
            }
            return retVal;
        }

    }
}
