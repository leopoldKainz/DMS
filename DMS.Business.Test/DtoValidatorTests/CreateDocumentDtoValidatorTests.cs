using DMS.Business.Models.Validation;
using DMS.DAL.DTOs;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class CreateDocumentDtoValidatorTests
{
    private CreateDocumentDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateDocumentDtoValidator();
    }

    [Test]
    public void Validate_ValidDocumentDto_PassesValidation()
    {
        // Arrange
        var validDto = new CreateDocumentDto("report.pdf", "application/pdf", 1024);

        // Act
        var result = _validator.TestValidate(validDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_FaultyDocumentDto_FailsValidation()
    {
        // Arrange: empty name, empty type, 0 size
        var faultyDto = new CreateDocumentDto("", "", 0);

        // Act
        var result = _validator.TestValidate(faultyDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }
}
