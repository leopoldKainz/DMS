using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace DMS.Business.Test;

public class DocumentServiceTests
{
    private Mock<DmsDbContext> _mockContext = null!;
    private Mock<DbSet<Document>> _mockDocuments = null!;
    private Mock<DbSet<UserDocument>> _mockUserDocuments = null!;

    private User _user = null!;
    private Document _invoice = null!;
    private Document _contract = null!;
    private Guid _userId;
    private Guid _invoiceId;
    private Guid _contractId;

    [SetUp]
    public void Setup()
    {
        _userId = Guid.NewGuid();
        _invoiceId = Guid.NewGuid();
        _contractId = Guid.NewGuid();

        _user = new User { Id = 1, EntityId = _userId, Username = "max" };
        _invoice = new Document { Id = 10, EntityId = _invoiceId, Name = "invoice.pdf", Type = "application/pdf" };
        _contract = new Document { Id = 11, EntityId = _contractId, Name = "contract.pdf", Type = "application/pdf" };

        var userDocuments = new List<UserDocument>
        {
            new() { UserId = 1, User = _user, DocumentId = 10, Document = _invoice, Permission = Permission.Owner },
            new() { UserId = 1, User = _user, DocumentId = 11, Document = _contract, Permission = Permission.User }
        };

        var mockUsers = new List<User> { _user }.BuildMockDbSet();
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
        var result = await service.AddForOwnerAsync(_userId, newDocument);

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
        var unknownUserId = Guid.NewGuid();

        // Act + Assert
        Assert.ThrowsAsync<ArgumentException>(() => service.AddForOwnerAsync(unknownUserId, newDocument));
        _mockDocuments.Verify(s => s.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void AddForOwnerAsync_InvalidDocument_ThrowsValidationException()
    {
        // Arrange: valid user exists, but document has empty name/type and negative size
        var service = new DocumentService(_mockContext.Object);
        var invalidDocument = new Document { Name = "", Type = "", Size = -5 };

        // Act + Assert
        Assert.ThrowsAsync<ValidationException>(() => service.AddForOwnerAsync(_userId, invalidDocument));
        _mockDocuments.Verify(s => s.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByUserAsync_ReturnsAllDocumentsOfUser()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);

        // Act
        var result = await service.GetByUserAsync(_userId);

        // Assert
        Assert.That(result, Is.EquivalentTo(new[] { _invoice, _contract }));
    }

    [Test]
    public async Task HasAccessAsync_NotLinkedDocument_ReturnsFalse()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);

        // Act
        var result = await service.HasAccessAsync(Guid.NewGuid(), _invoiceId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RemoveByIdAsync_UnknownId_ReturnsFalse()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object);

        // Act
        var result = await service.RemoveByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
        _mockDocuments.Verify(s => s.Remove(It.IsAny<Document>()), Times.Never);
    }
}
