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
    public async Task AddAsync_ValidUser_HashesPasswordAndSaves()
    {
        // Arrange
        var mockSet = SetupUsers();
        var service = new UserService(_mockContext.Object, NullLogger<UserService>.Instance);
        var user = new User { Username = "max", Email = "max@test.at", Password = "secret" };

        // Act
        var result = await service.AddAsync(user);

        // Assert
        Assert.That(result.Password, Is.EqualTo(UserService.HashPassword("secret")));
        mockSet.Verify(s => s.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        Assert.That(user.Password, Is.EqualTo(UserService.HashPassword("new")));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
