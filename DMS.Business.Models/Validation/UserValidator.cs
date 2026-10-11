using DMS.DAL.Entities;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Username).ValidUsername();
        RuleFor(x => x.Email!).ValidEmail().When(x => x.Email != null);
        RuleFor(x => x.Password).ValidPassword();
    }
}
