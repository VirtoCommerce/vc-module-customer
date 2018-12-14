using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class ContactDataEntity : MemberDataEntity
    {
        public ContactDataEntity()
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

        [StringLength(2083)]
        public string PhotoUrl { get; set; }

        [StringLength(256)]
        public string Salutation { get; set; }

        #endregion

        public override Member ToModel(Member member)
        {
            //Call base converter first
            base.ToModel(member);
            var contact = member as Contact;
            if (contact != null)
            {
                contact.Organizations = MemberRelations.Select(x => x.Ancestor).OfType<OrganizationDataEntity>().Select(x => x.Id).ToList();
                member.Name = contact.FullName;
            }
            return member;
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            var contact = member as Contact;
            if (contact != null)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    Name = contact.FullName;
                }

                if (contact.Organizations != null)
                {
                    MemberRelations = new ObservableCollection<MemberRelationDataEntity>();
                    foreach (var organization in contact.Organizations)
                    {
                        var memberRelation = new MemberRelationDataEntity
                        {
                            AncestorId = organization,
                            AncestorSequence = 1,
                            DescendantId = Id,
                        };
                        MemberRelations.Add(memberRelation);
                    }
                }
            }
            //Call base converter
            return base.FromModel(member, pkMap);
        }

        public override void Patch(MemberDataEntity target)
        {
            if (target is ContactDataEntity targetContact)
            {
                targetContact.FirstName = FirstName;
                targetContact.MiddleName = MiddleName;
                targetContact.LastName = LastName;
                targetContact.BirthDate = BirthDate;
                targetContact.DefaultLanguage = DefaultLanguage;
                targetContact.FullName = FullName;
                targetContact.Salutation = Salutation;
                targetContact.TimeZone = TimeZone;
                targetContact.TaxpayerId = TaxpayerId;
                targetContact.PreferredCommunication = PreferredCommunication;
                targetContact.PreferredDelivery = PreferredDelivery;
                targetContact.PhotoUrl = PhotoUrl;
            }
            base.Patch(target);
        }
    }
}
