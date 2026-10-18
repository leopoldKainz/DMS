using System.Security.Cryptography;
using System.Text;
using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;
using Moq;

namespace DMS.Business.Test;

public class UserServiceTests
{
    private Mock<DmsDbContext> _mockContext = null!;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<DmsDbContext>(new DbContextOptions<DmsDbContext>());
    }

    private Mock<DbSet<User>> SetupUsers(params User[] users)
    {
        var mockSet = users.ToList().BuildMockDbSet();
        _mockContext.Setup(c => c.Set<User>()).Returns(mockSet.Object);
        return mockSet;
    }

    [Test]
    public async Task AddAsync_ValidUser_HashesPasswordWithoutSaving()
    {
        // Arrange
        var mockSet = SetupUsers();
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);
        var user = new User { Username = "max", Email = "max@test.at", Password = "secret" };

        // Act
        var result = await service.AddAsync(user);

        // Assert
        Assert.That(result.Password, Is.Not.EqualTo("secret"));
        Assert.That(UserService.VerifyPassword("secret", result.Password), Is.True);
        mockSet.Verify(s => s.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void AddAsync_EmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        SetupUsers();
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);
        var user = new User { Username = "max", Email = "max@test.at", Password = "" };

        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(() => service.AddAsync(user));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void AddAsync_PasswordTooShort_ThrowsValidationExceptionAndDoesNotSave()
    {
        var mockSet = SetupUsers();
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);
        var user = new User { Username = "max", Email = "max@test.at", Password = "12345" };

        Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(user));
        mockSet.Verify(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void AddAsync_InvalidUserEntity_ThrowsValidationException()
    {
        // Arrange
        SetupUsers();
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);
        // Faulty user entity: empty username, invalid email format
        var user = new User { Username = "", Email = "not-an-email", Password = "secret" };

        // Act + Assert
        Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(user));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AuthenticateAsync_CorrectPassword_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = 1, Username = "max", Email = "max@test.at", Password = UserService.HashPassword("secret") };
        SetupUsers(user);
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);

        // Act
        var result = await service.AuthenticateAsync("max", "secret");

        // Assert
        Assert.That(result, Is.SameAs(user));
    }

    [Test]
    public async Task AuthenticateAsync_WrongPassword_ReturnsNull()
    {
        // Arrange
        SetupUsers(new User { Id = 1, Username = "max", Email = "max@test.at", Password = UserService.HashPassword("secret") });
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);

        // Act
        var result = await service.AuthenticateAsync("max", "wrong");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ChangePasswordAsync_CorrectCurrentPassword_UpdatesPassword()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var user = new User { Id = 1, EntityId = userGuid, Username = "max", Password = UserService.HashPassword("old") };
        SetupUsers(user);
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);

        // Act
        var result = await service.ChangePasswordAsync(userGuid, "old", "new");

        // Assert
        Assert.That(result, Is.True);
        Assert.That(UserService.VerifyPassword("new", user.Password), Is.True);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        var first = UserService.HashPassword("secret");
        var second = UserService.HashPassword("secret");

        Assert.That(first, Is.Not.EqualTo(second));
        Assert.That(UserService.VerifyPassword("secret", first), Is.True);
        Assert.That(UserService.VerifyPassword("secret", second), Is.True);
    }

    [Test]
    public async Task AuthenticateAsync_LegacySha256Hash_ReturnsUserAndUpgradesHash()
    {
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("secret")));
        var user = new User { Id = 1, Username = "max", Email = "max@test.at", Password = legacyHash };
        SetupUsers(user);
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);

        var result = await service.AuthenticateAsync("max", "secret");

        Assert.That(result, Is.SameAs(user));
        Assert.That(user.Password, Is.Not.EqualTo(legacyHash));
        Assert.That(UserService.VerifyPassword("secret", user.Password), Is.True);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AuthenticateAsync_LegacySha256HashWrongPassword_ReturnsNullAndKeepsHash()
    {
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("secret")));
        var user = new User { Id = 1, Username = "max", Email = "max@test.at", Password = legacyHash };
        SetupUsers(user);
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);

        var result = await service.AuthenticateAsync("max", "wrong");

        Assert.That(result, Is.Null);
        Assert.That(user.Password, Is.EqualTo(legacyHash));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
