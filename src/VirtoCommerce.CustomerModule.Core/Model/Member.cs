using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CustomerModule.Core.Model
{
    public abstract class Member : AuditableEntity, IHasDynamicProperties, ISeoSupport, IHasOuterId, ICloneable, IHasRelevanceScore
    {
        protected Member()
        {
            MemberType = GetType().Name;
        }

        public string Name { get; set; }
        public virtual string MemberType { get; set; }
        public string OuterId { get; set; }

        public string Status { get; set; }

        public IList<Address> Addresses { get; set; }
        public IList<string> Phones { get; set; }
        public IList<string> Emails { get; set; }
        public IList<Note> Notes { get; set; }
        public IList<string> Groups { get; set; }
        public string IconUrl { get; set; }

        public double? RelevanceScore { get; set; }

        #region IHasDynamicProperties Members

        public virtual string ObjectType => typeof(Member).FullName;
        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }

        public bool UseDynamicPropertyAccessor { get; set; } = false;

        private DynamicPropertyAccessor _dynamicPropertyAccessor;

        public DynamicPropertyAccessor DynamicPropertyAccessor
        {
            get
            {
                _dynamicPropertyAccessor ??= new DynamicPropertyAccessor(this);
                return _dynamicPropertyAccessor;
            }
            set
            {
                _dynamicPropertyAccessor = value;
                _dynamicPropertyAccessor.ConnectEntity(this, UseDynamicPropertyAccessor);
            }
        }

        #endregion IHasDynamicProperties Members

        #region ISeoSupport Members

        public virtual string SeoObjectType => GetType().Name;

        public virtual IList<SeoInfo> SeoInfos { get; set; }

        #endregion ISeoSupport Members

        public virtual void ReduceDetails(string responseGroup)
        {
            //Reduce details according to response group
            var memberResponseGroup = EnumUtility.SafeParseFlags(responseGroup, MemberResponseGroup.Full);

            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithNotes))
            {
                Notes = null;
            }
            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithAddresses))
            {
                Addresses = null;
            }
            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithEmails))
            {
                Emails = null;
            }
            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithGroups))
            {
                Groups = null;
            }
            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithPhones))
            {
                Phones = null;
            }
            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithSeo))
            {
                SeoInfos = null;
            }
            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithDynamicProperties))
            {
                DynamicProperties = null;
            }
        }

        #region ICloneable members

        public virtual object Clone()
        {
            var result = MemberwiseClone() as Member;

            result.Notes = Notes?.Select(x => x.Clone()).OfType<Note>().ToList();
            result.Addresses = Addresses?.Select(x => x.Clone()).OfType<Address>().ToList();
            result.SeoInfos = SeoInfos?.Select(x => x.Clone()).OfType<SeoInfo>().ToList();
            result.DynamicProperties = DynamicProperties?.Select(x => x.Clone()).OfType<DynamicObjectProperty>().ToList();

            return result;
        }

        #endregion ICloneable members
    }
}
