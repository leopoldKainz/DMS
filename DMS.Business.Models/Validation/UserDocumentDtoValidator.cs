using DMS.DAL.DTOs;
using DMS.DAL.Enums;
using FluentValidation;

namespace DMS.Business.Models.Validation;

public class UserDocumentDtoValidator : AbstractValidator<UserDocumentDto>
{
    public UserDocumentDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required.");

        RuleFor(x => x.DocumentName)
            .NotEmpty().WithMessage("Document name is required.")
            .MaximumLength(255).WithMessage("Document name cannot exceed 255 characters.");

        RuleFor(x => x.Permission)
            .NotEmpty().WithMessage("Permission is required.")
            .IsEnumName(typeof(Permission), caseSensitive: false).WithMessage("Permission must be a valid permission value.");

        RuleFor(x => x.CreatedAt)
            .NotEmpty().WithMessage("Creation timestamp is required.");
    }
}
