using DMS.Business.Models.Validation;
using DMS.DAL.Entities;
using FluentValidation.TestHelper;

namespace DMS.Business.Test;

public class UserValidatorTests
{
    private UserValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UserValidator();
    }

    [Test]
    public void Validate_ValidUser_PassesValidation()
    {
        // Arrange
        var user = new User
        {
            Username = "valid_user",
            Email = "valid@example.com",
            Password = "SecurePassword123"
        };

        // Act
        var result = _validator.TestValidate(user);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_FaultyUser_FailsValidation()
    {
        // Arrange: empty username, invalid email, short password
        var user = new User
        {
            Username = "",
            Email = "invalid-email",
            Password = "123"
        };

        // Act
        var result = _validator.TestValidate(user);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
