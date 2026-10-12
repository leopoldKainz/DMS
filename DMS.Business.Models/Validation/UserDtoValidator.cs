using DMS.DAL.DTOs;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.User.UserIdRequired);

        RuleFor(x => x.Username)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Username))
            .WithMessage(ValidationMessages.User.UsernameMaxLength);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage(ValidationMessages.User.EmailInvalid)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage(ValidationMessages.User.EmailMaxLength);

        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage(ValidationMessages.Common.CreatedAtRequired);
    }
}
