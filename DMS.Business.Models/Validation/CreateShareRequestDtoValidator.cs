using DMS.DAL.DTOs;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class CreateShareRequestDtoValidator : AbstractValidator<CreateShareRequestDto>
{
    public CreateShareRequestDtoValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage(ValidationMessages.ShareRequest.OwnerIdRequired);

        RuleFor(x => x.RecieverId)
            .NotEmpty().WithMessage(ValidationMessages.ShareRequest.ReceiverIdRequired)
            .NotEqual(x => x.OwnerId).WithMessage(ValidationMessages.ShareRequest.ReceiverCannotBeOwner);

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage(ValidationMessages.Document.DocumentIdRequired);
    }
}
