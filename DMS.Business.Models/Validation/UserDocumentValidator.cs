using DMS.DAL.Entities;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class UserDocumentValidator : AbstractValidator<UserDocument>
{
    public UserDocumentValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage(ValidationMessages.User.UserIdRequired);

        RuleFor(x => x.DocumentId)
            .GreaterThan(0).WithMessage(ValidationMessages.Document.DocumentIdRequired);

        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage(ValidationMessages.ShareRequest.PermissionRequired);
    }
}
