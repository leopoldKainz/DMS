using DMS.Business.Models.Validation;
using DMS.DAL.DTOs;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class LoginDtoValidatorTests
{
    private LoginDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new LoginDtoValidator();
    }

    [Test]
    public void Validate_ValidLoginDto_PassesValidation()
    {
        // Arrange: valid username and password
        var validDto = new LoginDto("john_doe", "SecurePassword123");

        // Act
        var result = _validator.TestValidate(validDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_FaultyLoginDto_FailsValidation()
    {
        // Arrange: empty username and password
        var faultyDto = new LoginDto("", "");

        // Act
        var result = _validator.TestValidate(faultyDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UsernameOrEmail);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
