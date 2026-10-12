using DMS.DAL.DTOs;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class ShareRequestDtoValidator : AbstractValidator<ShareRequestDto>
{
    public ShareRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.ShareRequest.IdRequired);

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage(ValidationMessages.ShareRequest.OwnerIdRequired);

        RuleFor(x => x.RecieverId)
            .NotEmpty().WithMessage(ValidationMessages.ShareRequest.ReceiverIdRequired)
            .NotEqual(x => x.OwnerId).WithMessage(ValidationMessages.ShareRequest.ReceiverCannotBeOwner);

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage(ValidationMessages.Document.DocumentIdRequired);

        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage(ValidationMessages.Common.CreatedAtRequired);
    }
}
