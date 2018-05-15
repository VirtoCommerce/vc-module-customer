using System;
using System.ComponentModel.DataAnnotations;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class AddressDataEntity : AuditableEntity
    {
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
            address.Key = Id;
            address.Phone = DaytimePhoneNumber;
            address.AddressType = EnumUtility.SafeParse(Type, AddressType.BillingAndShipping);
            return address;
        }

        public virtual AddressDataEntity FromModel(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");
         
            this.InjectFrom(address);
            Id = address.Key;
            DaytimePhoneNumber = address.Phone;
            Type = address.AddressType.ToString();
            return this;
        }

        public virtual void Patch(AddressDataEntity target)
        {
            target.City = City;
            target.CountryCode = CountryCode;
            target.CountryName = CountryName;
            target.DaytimePhoneNumber = DaytimePhoneNumber;
            target.PostalCode = PostalCode;
            target.RegionId = RegionId;
            target.RegionName = RegionName;
            target.Type = Type;
            target.City = City;
            target.Email = Email;
            target.FirstName = FirstName;
            target.LastName = LastName;
            target.Line1 = Line1;
            target.Line2 = Line2;
        }

        public override bool Equals(object obj)
        {
            var result = base.Equals(obj);
            //For transient addresses need to compare two objects as value object (by content)
            if (!result && IsTransient() && obj is AddressDataEntity otherAddressEntity)
            {
                var domainAddress = ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
                var otherAddress = otherAddressEntity.ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
                result = domainAddress.Equals(otherAddress);
            }
            return result;
        }

        public override int GetHashCode()
        {
            if (IsTransient())
            {
                //need to convert to domain address model to allow use ValueObject.GetHashCode
                var domainAddress = ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
                return domainAddress.GetHashCode();
            }
            return base.GetHashCode();
        }
     
    }
}
