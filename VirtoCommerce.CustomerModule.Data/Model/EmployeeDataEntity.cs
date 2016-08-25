using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;

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

        public DateTime? BirthDate { get; set; }

        public override Member ToModel(Member member)
        {
            //Call base converter first
            base.ToModel(member);

            var employee = member as Employee;
            employee.Organizations = this.MemberRelations.Select(x => x.Ancestor).OfType<OrganizationDataEntity>().Select(x => x.Id).ToList();
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
                    this.MemberRelations = new ObservableCollection<MemberRelationDataEntity>();
                    foreach (var organization in employee.Organizations)
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

        public override void Patch(MemberDataEntity memberEntity)
        {
            var target = memberEntity as EmployeeDataEntity;

            target.FirstName = this.FirstName;
            target.MiddleName = this.MiddleName;
            target.LastName = this.LastName;
            target.BirthDate = this.BirthDate;
            target.DefaultLanguage = this.DefaultLanguage;
            target.FullName = this.FullName;
            target.IsActive = this.IsActive;
            target.Type = this.Type;
            target.TimeZone = this.TimeZone;
          
            base.Patch(target);
        }
    }
}
