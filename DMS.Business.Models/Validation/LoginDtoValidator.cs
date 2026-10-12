using DMS.DAL.DTOs;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage(ValidationMessages.User.UsernameOrEmailRequired)
            .MaximumLength(50).WithMessage(ValidationMessages.User.UsernameOrEmailMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.User.PasswordRequired);
    }
}
