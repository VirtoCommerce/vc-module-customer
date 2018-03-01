using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Customer.Model;

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

        public byte[] Photo { get; set; }

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
                contact.Organizations = this.MemberRelations.Select(x => x.Ancestor).OfType<OrganizationDataEntity>().Select(x => x.Id).ToList();
                member.Name = contact.FullName;
            }
            return member;
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            var contact = member as Contact;
            if (contact != null)
            {
                if (string.IsNullOrEmpty(this.Name))
                {
                    this.Name = contact.FullName;
                }

                if (contact.Organizations != null)
                {
                    this.MemberRelations = new ObservableCollection<MemberRelationDataEntity>();
                    foreach (var organization in contact.Organizations)
                    {
                        var memberRelation = new MemberRelationDataEntity
                        {
                            AncestorId = organization,
                            AncestorSequence = 1,
                            DescendantId = this.Id,
                        };
                        this.MemberRelations.Add(memberRelation);
                    }
                }
            }
            //Call base converter
            return base.FromModel(member, pkMap);
        }

        public override void Patch(MemberDataEntity memberDataEntity)
        {
            var target = memberDataEntity as ContactDataEntity;

            target.FirstName = this.FirstName;
            target.MiddleName = this.MiddleName;
            target.LastName = this.LastName;
            target.BirthDate = this.BirthDate;
            target.DefaultLanguage = this.DefaultLanguage;
            target.FullName = this.FullName;
            target.Salutation = this.Salutation;
            target.TimeZone = this.TimeZone;
            target.TaxpayerId = this.TaxpayerId;
            target.PreferredCommunication = this.PreferredCommunication;
            target.PreferredDelivery = this.PreferredDelivery;
            target.Photo = this.Photo;

            base.Patch(target);
        }
    }
}
