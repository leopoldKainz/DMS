using DMS.DAL.Entities;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Document.NameRequired)
            .MaximumLength(255).WithMessage(ValidationMessages.Document.NameMaxLength);

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage(ValidationMessages.Document.TypeRequired)
            .MaximumLength(100).WithMessage(ValidationMessages.Document.TypeMaxLength);

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.Document.SizeNonNegative);
    }
}
