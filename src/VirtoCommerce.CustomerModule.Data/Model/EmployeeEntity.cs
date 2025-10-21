using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class EmployeeEntity : MemberEntity, IHasOrganizationsEntity
    {
        [StringLength(64)]
        public string Type { get; set; }

        public bool IsActive { get; set; }

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

        [StringLength(2083)]
        public string PhotoUrl { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(128)]
        public string DefaultOrganizationId { get; set; }

        [StringLength(128)]
        public string CurrentOrganizationId { get; set; }

        public override Member ToModel(Member member)
        {
            // Call base converter first
            base.ToModel(member);

            if (member is Employee employee)
            {
                employee.FirstName = FirstName;
                employee.MiddleName = MiddleName;
                employee.LastName = LastName;
                employee.BirthDate = BirthDate;
                employee.DefaultLanguage = DefaultLanguage;
                employee.FullName = FullName;
                employee.IsActive = IsActive;
                employee.EmployeeType = Type;
                employee.TimeZone = TimeZone;
                employee.PhotoUrl = PhotoUrl;
                employee.Organizations = MemberRelations
                    .Where(x => x.RelationType == RelationType.Membership.ToString())
                    .Select(x => x.Ancestor)
                    .OfType<OrganizationEntity>()
                    .Select(x => x.Id)
                    .ToList();
                employee.DefaultOrganizationId = DefaultOrganizationId;
                employee.CurrentOrganizationId = CurrentOrganizationId;
            }

            return member;
        }

        public override MemberEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            // Call base converter first
            base.FromModel(member, pkMap);

            if (member is Employee employee)
            {
                member.Name = employee.FullName;
                FirstName = employee.FirstName;
                MiddleName = employee.MiddleName;
                LastName = employee.LastName;
                BirthDate = employee.BirthDate;
                DefaultLanguage = employee.DefaultLanguage;
                FullName = employee.FullName;
                IsActive = employee.IsActive;
                Type = employee.MemberType;
                TimeZone = employee.TimeZone;
                PhotoUrl = employee.PhotoUrl;
                DefaultOrganizationId = employee.DefaultOrganizationId;
                CurrentOrganizationId = employee.CurrentOrganizationId;

                if (employee.Organizations != null)
                {
                    MemberRelations = new ObservableCollection<MemberRelationEntity>();
                    foreach (var organization in employee.Organizations)
                    {
                        var memberRelation = AbstractTypeFactory<MemberRelationEntity>.TryCreateInstance();
                        memberRelation.AncestorId = organization;
                        memberRelation.AncestorSequence = 1;
                        memberRelation.DescendantId = Id;
                        memberRelation.RelationType = RelationType.Membership.ToString();

                        MemberRelations.Add(memberRelation);
                    }
                }
            }

            return this;
        }

        public override void Patch(MemberEntity target)
        {
            base.Patch(target);

            if (target is EmployeeEntity employee)
            {
                employee.FirstName = FirstName;
                employee.MiddleName = MiddleName;
                employee.LastName = LastName;
                employee.BirthDate = BirthDate;
                employee.DefaultLanguage = DefaultLanguage;
                employee.FullName = FullName;
                employee.IsActive = IsActive;
                employee.Type = Type;
                employee.TimeZone = TimeZone;
                employee.PhotoUrl = PhotoUrl;
                employee.DefaultOrganizationId = DefaultOrganizationId;
                employee.CurrentOrganizationId = CurrentOrganizationId;
            }
        }
    }
}
