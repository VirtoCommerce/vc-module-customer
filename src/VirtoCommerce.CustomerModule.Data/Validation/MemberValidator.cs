using System.Linq;
using FluentValidation;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Validation
{
    public class MemberValidator : AbstractValidator<Member>
    {
        private const string MemberTypeMessage = "'{0}' is an incorrect value for the member type";

        public MemberValidator()
        {
            var typeNames = AbstractTypeFactory<Member>.AllTypeInfos.Select(x => x.Type.Name).ToHashSet();

            RuleFor(member => member.MemberType)
                .NotNull()
                .NotEmpty()
                .Must(x => typeNames.Contains(x))
                .WithMessage(x => string.Format(MemberTypeMessage, x.MemberType));
        }
    }
}
