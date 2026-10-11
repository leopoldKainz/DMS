using DMS.DAL.DTOs;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class CreateDocumentDtoValidator : AbstractValidator<CreateDocumentDto>
{
    public CreateDocumentDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Document.NameRequired)
            .MaximumLength(255).WithMessage(ValidationMessages.Document.NameMaxLength);

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage(ValidationMessages.Document.TypeRequired)
            .MaximumLength(100).WithMessage(ValidationMessages.Document.TypeMaxLength);

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage(ValidationMessages.Document.SizePositive);
    }
}
