using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class ContactEntity : MemberEntity
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

        [StringLength(254)]
        [Required]
        public string FullName { get; set; }

        [StringLength(32)]
        public string TimeZone { get; set; }

        [StringLength(32)]
        public string DefaultLanguage { get; set; }

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

        #endregion

        public override Member ToModel(Member member)
        {
            //Call base converter first
            base.ToModel(member);
            if (member is Contact contact)
            {
                contact.FirstName = FirstName;
                contact.MiddleName = MiddleName;
                contact.LastName = LastName;
                contact.BirthDate = BirthDate;
                contact.DefaultLanguage = DefaultLanguage;
                contact.FullName = FullName;
                contact.Salutation = Salutation;
                contact.TimeZone = TimeZone;
                contact.TaxPayerId = TaxpayerId;
                contact.PreferredCommunication = PreferredCommunication;
                contact.PreferredDelivery = PreferredDelivery;
                contact.DefaultShippingAddressId = DefaultShippingAddressId;
                contact.DefaultBillingAddressId = DefaultBillingAddressId;
                contact.PhotoUrl = PhotoUrl;
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
            }
            return member;
        }

        public override MemberEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            if (member is Contact contact)
            {
                FirstName = contact.FirstName;
                MiddleName = contact.MiddleName;
                LastName = contact.LastName;
                BirthDate = contact.BirthDate;
                DefaultLanguage = contact.DefaultLanguage;
                FullName = member.Name = contact.FullName;
                Salutation = contact.Salutation;
                TimeZone = contact.TimeZone;
                TaxpayerId = contact.TaxPayerId;
                PreferredCommunication = contact.PreferredCommunication;
                PreferredDelivery = contact.PreferredDelivery;
                DefaultShippingAddressId = contact.DefaultShippingAddressId;
                DefaultBillingAddressId = contact.DefaultBillingAddressId;
                PhotoUrl = contact.PhotoUrl;

                if (string.IsNullOrEmpty(Name))
                {
                    Name = contact.FullName;
                }

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
            //Call base converter
            return base.FromModel(member, pkMap);
        }

        public override void Patch(MemberEntity target)
        {
            if (target is ContactEntity contact)
            {
                contact.FirstName = FirstName;
                contact.MiddleName = MiddleName;
                contact.LastName = LastName;
                contact.BirthDate = BirthDate;
                contact.DefaultLanguage = DefaultLanguage;
                contact.FullName = FullName;
                contact.Salutation = Salutation;
                contact.TimeZone = TimeZone;
                contact.TaxpayerId = TaxpayerId;
                contact.PreferredCommunication = PreferredCommunication;
                contact.PreferredDelivery = PreferredDelivery;
                contact.DefaultShippingAddressId = DefaultShippingAddressId;
                contact.DefaultBillingAddressId = DefaultBillingAddressId;
                contact.PhotoUrl = PhotoUrl;
            }

            base.Patch(target);
        }
    }
}
