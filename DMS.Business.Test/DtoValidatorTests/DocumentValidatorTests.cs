using DMS.Business.Models.Validation;
using DMS.DAL.Entities;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class DocumentValidatorTests
{
    private DocumentValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new DocumentValidator();
    }

    [Test]
    public void Validate_ValidDocument_PassesValidation()
    {
        // Arrange
        var document = new Document
        {
            Name = "manual.pdf",
            Type = "application/pdf",
            Size = 2048
        };

        // Act
        var result = _validator.TestValidate(document);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_FaultyDocument_FailsValidation()
    {
        // Arrange: empty name, empty type, negative size
        var document = new Document
        {
            Name = "",
            Type = "",
            Size = -1
        };

        // Act
        var result = _validator.TestValidate(document);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }
}
