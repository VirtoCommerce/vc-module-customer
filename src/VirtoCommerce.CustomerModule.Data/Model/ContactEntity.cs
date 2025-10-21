using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class ContactEntity : MemberEntity, IHasOrganizationsEntity
    {
        public ContactEntity()
        {
            BirthDate = DateTime.Now;
        }

        #region UserProfile members

        [StringLength(128)]
        public string FirstName { get; set; }

        [StringLength(128)]
        public string MiddleName { get; set; }

        [StringLength(128)]
        public string LastName { get; set; }

        [StringLength(512)]
        [Required]
        public string FullName { get; set; }

        [StringLength(32)]
        public string TimeZone { get; set; }

        [StringLength(32)]
        public string DefaultLanguage { get; set; }

        [StringLength(32)]
        public string CurrencyCode { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(64)]
        public string TaxpayerId { get; set; }

        [StringLength(64)]
        public string PreferredDelivery { get; set; }

        [StringLength(64)]
        public string PreferredCommunication { get; set; }

        [StringLength(128)]
        public string DefaultShippingAddressId { get; set; }

        [StringLength(128)]
        public string DefaultBillingAddressId { get; set; }

        [StringLength(2083)]
        public string PhotoUrl { get; set; }

        [StringLength(256)]
        public string Salutation { get; set; }

        public bool IsAnonymized { get; set; }

        public string About { get; set; }

        [StringLength(128)]
        public string DefaultOrganizationId { get; set; }

        [StringLength(128)]
        public string CurrentOrganizationId { get; set; }

        [Obsolete("Use GetSelectedAddressId() or SaveSelectedAddressId() from VirtoCommerce.CustomerModule.Core.Extensions.CustomerPreferenceServiceExtensions", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        [StringLength(128)]
        public string SelectedAddressId { get; set; }

        #endregion UserProfile members

        public override Member ToModel(Member member)
        {
            // Call base converter first
            base.ToModel(member);

            if (member is Contact contact)
            {
                contact.FirstName = FirstName;
                contact.MiddleName = MiddleName;
                contact.LastName = LastName;
                contact.BirthDate = BirthDate;
                contact.DefaultLanguage = DefaultLanguage;
                contact.CurrencyCode = CurrencyCode;
                contact.FullName = FullName;
                contact.Salutation = Salutation;
                contact.TimeZone = TimeZone;
                contact.TaxPayerId = TaxpayerId;
                contact.PreferredCommunication = PreferredCommunication;
                contact.PreferredDelivery = PreferredDelivery;
                contact.DefaultShippingAddressId = DefaultShippingAddressId;
                contact.DefaultBillingAddressId = DefaultBillingAddressId;
                contact.PhotoUrl = PhotoUrl;
                contact.IsAnonymized = IsAnonymized;
                contact.Organizations = MemberRelations
                    .Where(x => x.RelationType == RelationType.Membership.ToString())
                    .Select(x => x.Ancestor)
                    .OfType<OrganizationEntity>()
                    .Select(x => x.Id)
                    .ToList();
                contact.AssociatedOrganizations = MemberRelations
                    .Where(x => x.RelationType == RelationType.Association.ToString())
                    .Select(x => x.Ancestor)
                    .OfType<OrganizationEntity>()
                    .Select(x => x.Id)
                    .ToList();
                contact.Name = contact.FullName;
                contact.About = About;
                contact.DefaultOrganizationId = DefaultOrganizationId;
                contact.CurrentOrganizationId = CurrentOrganizationId;
#pragma warning disable VC0011 // Type or member is obsolete
                contact.SelectedAddressId = SelectedAddressId;
#pragma warning restore VC0011 // Type or member is obsolete
            }
            return member;
        }

        public override MemberEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            // Call base converter first
            base.FromModel(member, pkMap);

            if (member is Contact contact)
            {
                FirstName = contact.FirstName;
                MiddleName = contact.MiddleName;
                LastName = contact.LastName;
                BirthDate = contact.BirthDate;
                DefaultLanguage = contact.DefaultLanguage;
                CurrencyCode = contact.CurrencyCode;
                FullName = member.Name = contact.FullName;
                Salutation = contact.Salutation;
                TimeZone = contact.TimeZone;
                TaxpayerId = contact.TaxPayerId;
                PreferredCommunication = contact.PreferredCommunication;
                PreferredDelivery = contact.PreferredDelivery;
                DefaultShippingAddressId = contact.DefaultShippingAddressId;
                DefaultBillingAddressId = contact.DefaultBillingAddressId;
                PhotoUrl = contact.PhotoUrl;
                IsAnonymized = contact.IsAnonymized;
                About = contact.About;
                DefaultOrganizationId = contact.DefaultOrganizationId;
                CurrentOrganizationId = contact.CurrentOrganizationId;
#pragma warning disable VC0011 // Type or member is obsolete
                SelectedAddressId = contact.SelectedAddressId;
#pragma warning restore VC0011 // Type or member is obsolete

                if (contact.Organizations != null)
                {
                    if (MemberRelations.IsNullCollection())
                    {
                        MemberRelations = new ObservableCollection<MemberRelationEntity>();
                    }

                    foreach (var organization in contact.Organizations)
                    {
                        var memberRelation = AbstractTypeFactory<MemberRelationEntity>.TryCreateInstance();
                        memberRelation.AncestorId = organization;
                        memberRelation.AncestorSequence = 1;
                        memberRelation.DescendantId = Id;
                        memberRelation.RelationType = RelationType.Membership.ToString();

                        MemberRelations.Add(memberRelation);
                    }
                }

                if (contact.AssociatedOrganizations != null)
                {
                    if (MemberRelations.IsNullCollection())
                    {
                        MemberRelations = new ObservableCollection<MemberRelationEntity>();
                    }
                    foreach (var organization in contact.AssociatedOrganizations)
                    {
                        var memberRelation = AbstractTypeFactory<MemberRelationEntity>.TryCreateInstance();
                        memberRelation.AncestorId = organization;
                        memberRelation.AncestorSequence = 1;
                        memberRelation.DescendantId = Id;
                        memberRelation.RelationType = RelationType.Association.ToString();

                        MemberRelations.Add(memberRelation);
                    }
                }
            }

            return this;
        }

        public override void Patch(MemberEntity target)
        {
            base.Patch(target);

            if (target is ContactEntity contact)
            {
                contact.FirstName = FirstName;
                contact.MiddleName = MiddleName;
                contact.LastName = LastName;
                contact.BirthDate = BirthDate;
                contact.DefaultLanguage = DefaultLanguage;
                contact.CurrencyCode = CurrencyCode;
                contact.FullName = FullName;
                contact.Salutation = Salutation;
                contact.TimeZone = TimeZone;
                contact.TaxpayerId = TaxpayerId;
                contact.PreferredCommunication = PreferredCommunication;
                contact.PreferredDelivery = PreferredDelivery;
                contact.DefaultShippingAddressId = DefaultShippingAddressId;
                contact.DefaultBillingAddressId = DefaultBillingAddressId;
                contact.PhotoUrl = PhotoUrl;
                contact.IsAnonymized = IsAnonymized;
                contact.About = About;
                contact.DefaultOrganizationId = DefaultOrganizationId;
                contact.CurrentOrganizationId = CurrentOrganizationId;
#pragma warning disable VC0011 // Type or member is obsolete
                contact.SelectedAddressId = SelectedAddressId;
#pragma warning restore VC0011 // Type or member is obsolete
            }
        }
    }
}
