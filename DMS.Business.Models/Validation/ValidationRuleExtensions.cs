using FluentValidation;

namespace DMS.Business.Models.Validation;

public static class ValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(ValidationMessages.User.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.User.EmailInvalid)
            .MaximumLength(50).WithMessage(ValidationMessages.User.EmailMaxLength);
    }

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(ValidationMessages.User.PasswordRequired)
            .MinimumLength(6).WithMessage(ValidationMessages.User.PasswordMinLength);
    }

    public static IRuleBuilderOptions<T, string?> ValidUsername<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(ValidationMessages.User.UsernameRequired)
            .MaximumLength(20).WithMessage(ValidationMessages.User.UsernameMaxLength);
    }
}
