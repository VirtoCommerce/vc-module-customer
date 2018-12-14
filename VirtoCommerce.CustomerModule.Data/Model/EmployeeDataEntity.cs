using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class EmployeeDataEntity : MemberDataEntity
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

        [StringLength(254)]
        [Required]
        public string FullName { get; set; }

        [StringLength(32)]
        public string TimeZone { get; set; }

        [StringLength(32)]
        public string DefaultLanguage { get; set; }

        [StringLength(2083)]
        public string PhotoUrl { get; set; }

        public DateTime? BirthDate { get; set; }

        public override Member ToModel(Member member)
        {
            //Call base converter first
            base.ToModel(member);

            var employee = member as Employee;
            if (employee != null)
            {
                employee.Organizations = MemberRelations.Select(x => x.Ancestor).OfType<OrganizationDataEntity>().Select(x => x.Id).ToList();
            }
            return member;
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            var employee = member as Employee;
            if (employee != null)
            {
                member.Name = employee.FullName;

                if (employee.Organizations != null)
                {
                    MemberRelations = new ObservableCollection<MemberRelationDataEntity>();
                    foreach (var organization in employee.Organizations)
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
            if (target is EmployeeDataEntity targetEmployee)
            {
                targetEmployee.FirstName = FirstName;
                targetEmployee.MiddleName = MiddleName;
                targetEmployee.LastName = LastName;
                targetEmployee.BirthDate = BirthDate;
                targetEmployee.DefaultLanguage = DefaultLanguage;
                targetEmployee.FullName = FullName;
                targetEmployee.IsActive = IsActive;
                targetEmployee.Type = Type;
                targetEmployee.TimeZone = TimeZone;
                targetEmployee.PhotoUrl = PhotoUrl;
            }
            base.Patch(target);
        }
    }
}
