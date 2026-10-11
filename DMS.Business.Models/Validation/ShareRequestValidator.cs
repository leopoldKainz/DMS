using FluentValidation;

namespace DMS.Business.Models.Validation;

public class ShareRequestValidator : AbstractValidator<ShareRequest>
{
    public ShareRequestValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage(ValidationMessages.Document.DocumentIdRequired);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationMessages.User.UserIdRequired);

        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage(ValidationMessages.ShareRequest.PermissionRequired);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(ValidationMessages.ShareRequest.StatusRequired);
    }
}
