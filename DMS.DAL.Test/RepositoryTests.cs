using DMS.DAL.Entities;
using DMS.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace DMS.DAL.Test;

public class RepositoryTests
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
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        // Arrange
        var user = new User { Id = 1, Username = "max" };
        var mockSet = SetupUsers();
        mockSet.Setup(s => s.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);
        var repo = new Repository<User>(_mockContext.Object);

        // Act
        var result = await repo.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.SameAs(user));
    }

    [Test]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        // Arrange
        var mockSet = SetupUsers();
        mockSet.Setup(s => s.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((User?)null);
        var repo = new Repository<User>(_mockContext.Object);

        // Act
        var result = await repo.GetByIdAsync(99);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ListAsync_ReturnsAllEntities()
    {
        // Arrange
        SetupUsers(
            new User { Id = 1, Username = "max" },
            new User { Id = 2, Username = "anna" });
        var repo = new Repository<User>(_mockContext.Object);

        // Act
        var result = await repo.ListAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ListAsync_WithPredicate_ReturnsOnlyMatchingEntities()
    {
        // Arrange
        SetupUsers(
            new User { Id = 1, Username = "max" },
            new User { Id = 2, Username = "anna" },
            new User { Id = 3, Username = "max2" });
        var repo = new Repository<User>(_mockContext.Object);

        // Act
        var result = await repo.ListAsync(u => u.Username!.StartsWith("max"));

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(u => u.Username!.StartsWith("max")), Is.True);
    }

    [Test]
    public async Task AddAsync_CallsDbSetAddAsync()
    {
        // Arrange
        var mockSet = SetupUsers();
        var repo = new Repository<User>(_mockContext.Object);
        var user = new User { Username = "max" };

        // Act
        await repo.AddAsync(user);

        // Assert
        mockSet.Verify(s => s.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Update_CallsDbSetUpdate()
    {
        // Arrange
        var mockSet = SetupUsers();
        var repo = new Repository<User>(_mockContext.Object);
        var user = new User { Id = 1, Username = "max" };

        // Act
        repo.Update(user);

        // Assert
        mockSet.Verify(s => s.Update(user), Times.Once);
    }

    [Test]
    public void Remove_CallsDbSetRemove()
    {
        // Arrange
        var mockSet = SetupUsers();
        var repo = new Repository<User>(_mockContext.Object);
        var user = new User { Id = 1, Username = "max" };

        // Act
        repo.Remove(user);

        // Assert
        mockSet.Verify(s => s.Remove(user), Times.Once);
    }

    [Test]
    public async Task SaveChangesAsync_CallsContextAndReturnsAffectedRows()
    {
        // Arrange
        SetupUsers();
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(3);
        var repo = new Repository<User>(_mockContext.Object);

        // Act
        var result = await repo.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.EqualTo(3));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
