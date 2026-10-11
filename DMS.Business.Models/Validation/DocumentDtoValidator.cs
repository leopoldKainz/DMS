using DMS.DAL.DTOs;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class DocumentDtoValidator : AbstractValidator<DocumentDto>
{
    public DocumentDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.Document.DocumentIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Document.NameRequired)
            .MaximumLength(255).WithMessage(ValidationMessages.Document.NameMaxLength);

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage(ValidationMessages.Document.TypeRequired)
            .MaximumLength(100).WithMessage(ValidationMessages.Document.TypeMaxLength);

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.Document.SizeNonNegative);

        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage(ValidationMessages.Common.CreatedAtRequired);
    }
}
