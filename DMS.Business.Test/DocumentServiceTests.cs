using DMS.Business.Exceptions;
using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;
using Moq;

namespace DMS.Business.Test;

public class DocumentServiceTests
{
    private Mock<DmsDbContext> _mockContext = null!;
    private Mock<DbSet<Document>> _mockDocuments = null!;
    private Mock<DbSet<UserDocument>> _mockUserDocuments = null!;

    private User _user = null!;
    private User _otherUser = null!;
    private Document _invoice = null!;
    private Document _contract = null!;
    private Document _foreignDocument = null!;
    private Guid _userId;
    private Guid _invoiceId;
    private Guid _contractId;
    private Guid _foreignDocumentId;

    [SetUp]
    public void Setup()
    {
        _userId = Guid.NewGuid();
        _invoiceId = Guid.NewGuid();
        _contractId = Guid.NewGuid();
        _foreignDocumentId = Guid.NewGuid();

        _user = new User { Id = 1, EntityId = _userId, Username = "max" };
        _otherUser = new User { Id = 2, EntityId = Guid.NewGuid(), Username = "anna" };
        _invoice = new Document { Id = 10, EntityId = _invoiceId, Name = "invoice.pdf", Type = "application/pdf" };
        _contract = new Document { Id = 11, EntityId = _contractId, Name = "contract.pdf", Type = "application/pdf" };
        _foreignDocument = new Document { Id = 12, EntityId = _foreignDocumentId, Name = "secret.pdf", Type = "application/pdf" };

        var userDocuments = new List<UserDocument>
        {
            new() { UserId = 1, User = _user, DocumentId = 10, Document = _invoice, Permission = Permission.Owner },
            new() { UserId = 1, User = _user, DocumentId = 11, Document = _contract, Permission = Permission.User },
            new() { UserId = 2, User = _otherUser, DocumentId = 12, Document = _foreignDocument, Permission = Permission.Owner }
        };

        var mockUsers = new List<User> { _user, _otherUser }.BuildMockDbSet();
        _mockDocuments = new List<Document> { _invoice, _contract, _foreignDocument }.BuildMockDbSet();
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
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);
        var newDocument = new Document { Name = "new.pdf", Type = "application/pdf", Size = 50 };

        // Act
        var result = await service.AddForOwnerAsync(_userId, newDocument);

        // Assert
        Assert.That(result, Is.SameAs(newDocument));
        _mockDocuments.Verify(s => s.AddAsync(newDocument, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserDocuments.Verify(s => s.AddAsync(
            It.Is<UserDocument>(ud => ud.UserId == 1 && ud.Document == newDocument && ud.Permission == Permission.Owner),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void AddForOwnerAsync_UnknownUser_ThrowsNotFoundException()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);
        var newDocument = new Document { Name = "new.pdf", Type = "application/pdf" };
        var unknownUserId = Guid.NewGuid();

        // Act + Assert
        Assert.ThrowsAsync<NotFoundException>(() => service.AddForOwnerAsync(unknownUserId, newDocument));
        _mockDocuments.Verify(s => s.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void AddForOwnerAsync_InvalidDocument_ThrowsValidationException()
    {
        // Arrange: valid user exists, but document has empty name/type and negative size
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);
        var invalidDocument = new Document { Name = "", Type = "", Size = -5 };

        // Act + Assert
        Assert.ThrowsAsync<ValidationException>(() => service.AddForOwnerAsync(_userId, invalidDocument));
        _mockDocuments.Verify(s => s.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByUserAsync_ReturnsAllDocumentsOfUser()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        // Act
        var result = await service.GetByUserAsync(_userId);

        // Assert
        Assert.That(result, Is.EquivalentTo(new[] { _invoice, _contract }));
    }

    [Test]
    public async Task HasAccessAsync_NotLinkedDocument_ReturnsFalse()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        // Act
        var result = await service.HasAccessAsync(Guid.NewGuid(), _invoiceId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RemoveByIdAsync_UnknownId_ReturnsFalse()
    {
        // Arrange
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        // Act
        var result = await service.RemoveByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
        _mockDocuments.Verify(s => s.Remove(It.IsAny<Document>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RemoveByIdAsync_ExistingId_RemovesWithoutSaving()
    {
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        var result = await service.RemoveByIdAsync(_invoiceId);

        Assert.That(result, Is.True);
        _mockDocuments.Verify(s => s.Remove(_invoice), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- SearchAsync ---

    [Test]
    public async Task SearchAsync_IdFromSearchIndex_ReturnsDocumentEvenIfNameDoesNotMatch()
    {
        // Arrange: Elasticsearch found "Leuchtturm" in the OCR text of invoice.pdf, the name does not contain it
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        // Act
        var result = await service.SearchAsync(_userId, "Leuchtturm", [_invoiceId]);

        // Assert
        Assert.That(result, Is.EquivalentTo(new[] { _invoice }));
    }

    [Test]
    public async Task SearchAsync_NameMatch_ReturnsDocumentNotInSearchIndex()
    {
        // Arrange: nothing from Elasticsearch (e.g. OCR still pending), but the name matches case-insensitively
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        // Act
        var result = await service.SearchAsync(_userId, "CONTRACT", []);

        // Assert
        Assert.That(result, Is.EquivalentTo(new[] { _contract }));
    }

    [Test]
    public async Task SearchAsync_ForeignDocumentFromSearchIndex_IsNotReturned()
    {
        // Arrange: Elasticsearch returns a document the user has no access to
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        // Act
        var result = await service.SearchAsync(_userId, "Leuchtturm", [_foreignDocumentId]);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SearchAsync_MatchByNameAndSearchIndex_ReturnsDocumentOnce()
    {
        // Arrange: both the name and the index match the same document
        var service = new DocumentService(_mockContext.Object, NullLogger<DocumentService>.Instance);

        // Act
        var result = await service.SearchAsync(_userId, "invoice", [_invoiceId]);

        // Assert
        Assert.That(result, Is.EquivalentTo(new[] { _invoice }));
    }
}
