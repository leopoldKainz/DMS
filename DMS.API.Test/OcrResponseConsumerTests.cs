using DMS.API.Consumers;
using DMS.Business.Models;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;
using Moq;

namespace DMS.API.Test;

[TestFixture]
public class OcrResponseConsumerTests
{
    private Mock<DmsDbContext> _mockDbContext = null!;
    private Mock<ConsumeContext<OcrDocumentResponse>> _mockConsumeContext = null!;
    private OcrResponseConsumer _consumer = null!;
    private List<Document> _documents = null!;

    [SetUp]
    public void SetUp()
    {
        _documents = new List<Document>();
        var mockDbSet = _documents.BuildMockDbSet();

        _mockDbContext = new Mock<DmsDbContext>(new DbContextOptions<DmsDbContext>());
        _mockDbContext.Setup(c => c.Set<Document>()).Returns(mockDbSet.Object);

        _mockConsumeContext = new Mock<ConsumeContext<OcrDocumentResponse>>();
        _consumer = new OcrResponseConsumer(_mockDbContext.Object, NullLogger<OcrResponseConsumer>.Instance);
    }

    [Test]
    public async Task Consume_StatusProcessing_UpdatesDocumentOcrStatus()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = 1,
            EntityId = docId,
            Name = "scan.pdf",
            Type = "pdf",
            OcrStatus = OcrStatus.Pending
        };
        _documents.Add(document);

        var message = new OcrDocumentResponse
        {
            JobId = Guid.NewGuid(),
            DocumentId = docId,
            Status = OcrStatus.Processing
        };
        _mockConsumeContext.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(_mockConsumeContext.Object);

        // Assert
        Assert.That(document.OcrStatus, Is.EqualTo(OcrStatus.Processing));
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consume_StatusDone_UpdatesDocumentOcrStatus()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = 1,
            EntityId = docId,
            Name = "scan.pdf",
            Type = "pdf",
            OcrStatus = OcrStatus.Processing
        };
        _documents.Add(document);

        var message = new OcrDocumentResponse
        {
            JobId = Guid.NewGuid(),
            DocumentId = docId,
            Status = OcrStatus.Done,
            ExtractedText = "Recognized content"
        };
        _mockConsumeContext.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(_mockConsumeContext.Object);

        // Assert
        Assert.That(document.OcrStatus, Is.EqualTo(OcrStatus.Done));
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consume_StatusFailed_UpdatesDocumentOcrStatus()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = 1,
            EntityId = docId,
            Name = "scan.pdf",
            Type = "pdf",
            OcrStatus = OcrStatus.Processing
        };
        _documents.Add(document);

        var message = new OcrDocumentResponse
        {
            JobId = Guid.NewGuid(),
            DocumentId = docId,
            Status = OcrStatus.Failed,
            ErrorMessage = "File corrupted"
        };
        _mockConsumeContext.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(_mockConsumeContext.Object);

        // Assert
        Assert.That(document.OcrStatus, Is.EqualTo(OcrStatus.Failed));
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consume_DocumentNotFound_DoesNotThrowAndDoesNotSave()
    {
        // Arrange
        var message = new OcrDocumentResponse
        {
            JobId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            Status = OcrStatus.Processing
        };
        _mockConsumeContext.Setup(c => c.Message).Returns(message);

        // Act
        await _consumer.Consume(_mockConsumeContext.Object);

        // Assert
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
