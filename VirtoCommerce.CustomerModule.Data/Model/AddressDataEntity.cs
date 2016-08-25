using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;

namespace VirtoCommerce.CustomerModule.Data.Model
{
	public class AddressDataEntity : AuditableEntity
    {
		[Required]
		[StringLength(2048)]
		public string Name { get; set; }
			
		[StringLength(128)]
		public string FirstName { get; set; }

		[StringLength(128)]
		public string LastName { get; set; }

		[Required]
		[StringLength(128)]
		public string Line1 { get; set; }

		[StringLength(128)]
		public string Line2 { get; set; }

		[Required]
		[StringLength(128)]
		public string City { get; set; }

		[Required]
		[StringLength(64)]
		public string CountryCode  { get; set; }

		[StringLength(128)]
		public string StateProvince { get; set; }


		[Required]
		[StringLength(128)]
		public string CountryName { get; set; }

		[Required]
		[StringLength(32)]
		public string PostalCode { get; set; }

		[StringLength(128)]
		public string RegionId { get; set; }


		[StringLength(128)]
		public string RegionName { get; set; }


		[StringLength(64)]
		public string Type { get; set; }

		[StringLength(64)]
		public string DaytimePhoneNumber { get; set; }

		[StringLength(64)]
		public string EveningPhoneNumber { get; set; }

		[StringLength(64)]
		public string FaxNumber { get; set; }

        [StringLength(256)]
		public string Email { get; set; }

		[StringLength(128)]
		public string Organization { get; set; }
		
		#region Navigation Properties

		public string MemberId { get; set; }

		public virtual MemberDataEntity Member { get; set; }


		#endregion

        #region Overrides

        public override string ToString()
        {
            return string.Format("{0} {1}, {2} {3}, {4}, {5} {6} {7}", 
                FirstName, LastName, Line1, Line2, City, StateProvince, PostalCode, CountryName);
            
        }
        #endregion


        public virtual Address ToModel(Address address)
        {
            address.InjectFrom(this);
            address.Phone = this.DaytimePhoneNumber;
            address.AddressType = EnumUtility.SafeParse(this.Type, AddressType.BillingAndShipping);
            return address;
        }

        public virtual AddressDataEntity FromModel(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");
         
            this.InjectFrom(address);
            this.DaytimePhoneNumber = address.Phone;
            this.Type = address.AddressType.ToString();
            return this;
        }

        public virtual void Patch(AddressDataEntity target)
        {
            target.City = this.City;
            target.CountryCode = this.CountryCode;
            target.CountryName = this.CountryName;
            target.DaytimePhoneNumber = this.DaytimePhoneNumber;
            target.PostalCode = this.PostalCode;
            target.RegionId = this.RegionId;
            target.RegionName = this.RegionName;
            target.Type = this.Type;
            target.City = this.City;
            target.Email = this.Email;
            target.FirstName = this.FirstName;
            target.LastName = this.LastName;
            target.Line1 = this.Line1;
            target.Line2 = this.Line2;
        }
    }
}
