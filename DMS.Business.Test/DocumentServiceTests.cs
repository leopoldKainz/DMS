using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace DMS.Business.Test;

public class DocumentServiceTests
{
    private Mock<DmsDbContext> _mockContext = null!;
    private Mock<DbSet<Document>> _mockDocuments = null!;
    private Mock<DbSet<UserDocument>> _mockUserDocuments = null!;

    private Document _invoice = null!;
    private Document _contract = null!;

    [SetUp]
    public void Setup()
    {
        _invoice = new Document { Id = 10, Name = "invoice.pdf", Type = "application/pdf" };
        _contract = new Document { Id = 11, Name = "contract.pdf", Type = "application/pdf" };

        var userDocuments = new List<UserDocument>
        {
            new() { UserId = 1, DocumentId = 10, Document = _invoice, Permission = Permission.Owner },
            new() { UserId = 1, DocumentId = 11, Document = _contract, Permission = Permission.User }
        };

        var mockUsers = new List<User> { new() { Id = 1, Username = "max" } }.BuildMockDbSet();
        _mockDocuments = new List<Document> { _invoice, _contract }.BuildMockDbSet();
        _mockUserDocuments = userDocuments.BuildMockDbSet();

        _mockContext = new Mock<DmsDbContext>(new DbContextOptions<DmsDbContext>());
        _mockContext.Setup(c => c.Set<User>()).Returns(mockUsers.Object);
        _mockContext.Setup(c => c.Set<Document>()).Returns(_mockDocuments.Object);
        _mockContext.Setup(c => c.Set<UserDocument>()).Returns(_mockUserDocuments.Object);
    }

    [Test]
    public async Task AddForOwnerAsync_ExistingUser_AddsDocumentWithOwnerPermission()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);
        var newDocument = new Document { Name = "new.pdf", Type = "application/pdf", Size = 50 };

        // Act
        var result = await service.AddForOwnerAsync(1, newDocument);

        // Assert
        Assert.That(result, Is.SameAs(newDocument));
        _mockDocuments.Verify(s => s.AddAsync(newDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserDocuments.Verify(s => s.AddAsync(
            It.Is<UserDocument>(ud => ud.UserId == 1 && ud.Permission == Permission.Owner),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void AddForOwnerAsync_UnknownUser_ThrowsArgumentException()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);
        var newDocument = new Document { Name = "new.pdf", Type = "application/pdf" };

        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(() => service.AddForOwnerAsync(99, newDocument));
        _mockDocuments.Verify(s => s.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByUserAsync_ReturnsAllDocumentsOfUser()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);

        // Act
        var result = await service.GetByUserAsync(1);

        // Assert
        Assert.That(result, Is.EquivalentTo(new[] { _invoice, _contract }));
    }

    [Test]
    public async Task HasAccessAsync_NotLinkedDocument_ReturnsFalse()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);

        // Act
        var result = await service.HasAccessAsync(2, 10);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RemoveByIdAsync_UnknownId_ReturnsFalse()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);

        // Act
        var result = await service.RemoveByIdAsync(99);

        // Assert
        Assert.That(result, Is.False);
        _mockDocuments.Verify(s => s.Remove(It.IsAny<Document>()), Times.Never);
    }
}
