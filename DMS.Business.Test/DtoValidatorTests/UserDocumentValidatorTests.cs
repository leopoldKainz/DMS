using DMS.Business.Models.Validation;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class UserDocumentValidatorTests
{
    private UserDocumentValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UserDocumentValidator();
    }

    [Test]
    public void Validate_ValidUserDocument_PassesValidation()
    {
        // Arrange
        var userDoc = new UserDocument
        {
            UserId = 1,
            DocumentId = 1,
            Permission = Permission.Owner
        };

        // Act
        var result = _validator.TestValidate(userDoc);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_FaultyUserDocument_FailsValidation()
    {
        // Arrange
        var userDoc = new UserDocument
        {
            UserId = 0,
            DocumentId = 0,
            Permission = (Permission)999
        };

        // Act
        var result = _validator.TestValidate(userDoc);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.DocumentId);
        result.ShouldHaveValidationErrorFor(x => x.Permission);
    }
}
