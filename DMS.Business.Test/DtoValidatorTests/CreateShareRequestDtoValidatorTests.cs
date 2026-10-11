using DMS.Business.Models.Validation;
using DMS.DAL.DTOs;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class CreateShareRequestDtoValidatorTests
{
    private CreateShareRequestDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateShareRequestDtoValidator();
    }

    [Test]
    public void Validate_ValidShareRequestDto_PassesValidation()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var validDto = new CreateShareRequestDto(ownerId, receiverId, documentId);

        // Act
        var result = _validator.TestValidate(validDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_FaultyShareRequestDto_FailsValidation()
    {
        // Arrange: empty GUIDs and receiver equals owner
        var sameId = Guid.Empty;
        var faultyDto = new CreateShareRequestDto(sameId, sameId, sameId);

        // Act
        var result = _validator.TestValidate(faultyDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.RecieverId);
        result.ShouldHaveValidationErrorFor(x => x.DocumentId);
    }
}
