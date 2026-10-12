using DMS.Business.Models.Validation;
using DMS.DAL.DTOs;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class CreateUserDtoValidatorTests
{
    private CreateUserDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateUserDtoValidator();
    }

    [Test]
    public void Validate_ValidUserDto_PassesValidation()
    {
        // Arrange: valid data meeting all rules
        var validDto = new CreateUserDto(
            Username: "john_doe",
            Email: "john.doe@example.com",
            Password: "SecurePassword123"
        );

        // Act
        var result = _validator.TestValidate(validDto);

        // Assert: no validation errors should exist
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_InvalidUserDto_FailsValidation()
    {
        // Arrange: invalid data violating username, email, and password rules
        var invalidDto = new CreateUserDto(
            Username: "",             // Empty username
            Email: "not-an-email",    // Invalid email format
            Password: "123"           // Password too short (< 6 characters)
        );

        // Act
        var result = _validator.TestValidate(invalidDto);

        // Assert: should have validation errors for each invalid property
        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
