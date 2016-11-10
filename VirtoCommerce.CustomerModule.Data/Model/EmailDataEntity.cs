using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{

    public class EmailDataEntity : Entity
    {
        [CustomValidation(typeof(EmailDataEntity), "ValidateEmailContent", ErrorMessage = "Email has error")]
        [StringLength(254)]
        [Index(IsUnique = false)]
        public string Address { get; set; }

        public bool IsValidated { get; set; }

        [StringLength(64)]
        public string Type { get; set; }


        #region Navigation Properties

        public string MemberId { get; set; }

        public virtual MemberDataEntity Member { get; set; }


        #endregion

        #region Validation

        public static ValidationResult ValidateEmailContent(string value, ValidationContext context)
        {
            if (value == null || string.IsNullOrEmpty(value))
            {
                return new ValidationResult("Email can't be empty");
            }

            Regex regex = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$");
            if (!regex.IsMatch(value))
            {
                return new ValidationResult((@"Email must be ""email@server.[domain 2].domain"));
            }
            else
            {
                return ValidationResult.Success;
            }

        }

        #endregion

    }


}
