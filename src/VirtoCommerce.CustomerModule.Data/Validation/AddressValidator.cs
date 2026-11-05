using FluentValidation;
using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Data.Validation
{
    public class AddressValidator : AbstractValidator<Address>
    {
        public const int LineMaxLength = 2048;

        public AddressValidator()
        {
            RuleFor(address => address.Line1).MaximumLength(LineMaxLength);
            RuleFor(address => address.Line2).MaximumLength(LineMaxLength);
        }
    }
}
