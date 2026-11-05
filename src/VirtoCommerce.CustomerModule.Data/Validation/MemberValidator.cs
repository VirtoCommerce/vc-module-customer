using FluentValidation;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Validation
{
    public class MemberValidator : AbstractValidator<Member>
    {
        public const int AddressLineMaxLength = 2048;

        private const string MemberTypeMessage = "'{0}' is an incorrect value for the member type";

        public MemberValidator()
        {
            RuleFor(member => member.MemberType)
                .NotNull()
                .NotEmpty()
                .Must(memberType => AbstractTypeFactory<Member>.FindTypeInfoByName(memberType) != null)
                .WithMessage(x => string.Format(MemberTypeMessage, x.MemberType));

            RuleForEach(member => member.Addresses)
                .ChildRules(address =>
                {
                    address.RuleFor(x => x.Line1).MaximumLength(AddressLineMaxLength);
                    address.RuleFor(x => x.Line2).MaximumLength(AddressLineMaxLength);
                });
        }
    }
}
