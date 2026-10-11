using DMS.Business.Models;
using DMS.Business.Models.Validation;
using DMS.DAL.Enums;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class ShareRequestValidatorTests
{
    private ShareRequestValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ShareRequestValidator();
    }

    [Test]
    public void Validate_ValidShareRequestModel_PassesValidation()
    {
        // Arrange
        var request = new ShareRequest
        {
            DocumentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Permission = Permission.User,
            Status = ShareRequestStatus.Pending
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_FaultyShareRequestModel_FailsValidation()
    {
        // Arrange
        var request = new ShareRequest
        {
            DocumentId = Guid.Empty,
            UserId = Guid.Empty,
            Permission = (Permission)999,
            Status = (ShareRequestStatus)999
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentId);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.Permission);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
