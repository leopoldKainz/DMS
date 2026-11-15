using System.Text;
using DMS.Business.Models;
using DMS.DAL.Enums;
using DMS.ELK;
using DMS.ELK.Interfaces;
using DMS.OCR.Consumers;
using DMS.OCR.Services;
using DMS.Storage;
using DMS.Storage.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace DMS.OCR.Test;

[TestFixture]
public class OcrWorkerConsumerTests
{
    private Mock<IFileStorage> _fileStorageMock = null!;
    private Mock<ISearchIndex> _searchIndexMock = null!;
    private Mock<IOcrService> _ocrServiceMock = null!;
    private Mock<ILogger<OcrWorkerConsumer>> _loggerMock = null!;
    private Mock<ConsumeContext<ProcessOcrDocument>> _consumeContextMock = null!;
    private OcrWorkerConsumer _consumer = null!;

    [SetUp]
    public void SetUp()
    {
        _fileStorageMock = new Mock<IFileStorage>();
        _searchIndexMock = new Mock<ISearchIndex>();
        _ocrServiceMock = new Mock<IOcrService>();
        _loggerMock = new Mock<ILogger<OcrWorkerConsumer>>();
        _consumeContextMock = new Mock<ConsumeContext<ProcessOcrDocument>>();

        _consumer = new OcrWorkerConsumer(
            _fileStorageMock.Object,
            _searchIndexMock.Object,
            _ocrServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Consume_SuccessfulOcr_PublishesProcessingAndDone_AndIndexesInElasticsearch()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var message = new ProcessOcrDocument
        {
            JobId = jobId,
            DocumentId = documentId,
            DocumentName = "contract.pdf"
        };
        _consumeContextMock.Setup(x => x.Message).Returns(message);
        _consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var fileBytes = Encoding.UTF8.GetBytes("%PDF-1.4 dummy pdf content");
        var memoryStream = new MemoryStream(fileBytes);
        var storedFile = new StoredFile(memoryStream, "application/pdf", fileBytes.Length);

        _fileStorageMock
            .Setup(x => x.DownloadAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedFile);

        const string recognizedText = "Dies ist ein deutscher und englischer Beispieltext. Handgeschrieben.";
        _ocrServiceMock
            .Setup(x => x.ProcessPdfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recognizedText);

        var publishedResponses = new List<OcrDocumentResponse>();
        _consumeContextMock
            .Setup(x => x.Publish(It.IsAny<OcrDocumentResponse>(), It.IsAny<CancellationToken>()))
            .Callback<OcrDocumentResponse, CancellationToken>((resp, _) => publishedResponses.Add(resp))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_consumeContextMock.Object);

        // Assert
        _fileStorageMock.Verify(x => x.DownloadAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
        _ocrServiceMock.Verify(x => x.ProcessPdfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _searchIndexMock.Verify(x => x.IndexAsync(
            It.Is<SearchDocument>(doc => doc.DocumentId == documentId && doc.Name == "contract.pdf" && doc.Content == recognizedText),
            It.IsAny<CancellationToken>()), Times.Once);

        Assert.That(publishedResponses.Count, Is.EqualTo(2));
        Assert.That(publishedResponses[0].Status, Is.EqualTo(OcrStatus.Processing));
        Assert.That(publishedResponses[0].DocumentId, Is.EqualTo(documentId));

        Assert.That(publishedResponses[1].Status, Is.EqualTo(OcrStatus.Done));
        Assert.That(publishedResponses[1].DocumentId, Is.EqualTo(documentId));
        Assert.That(publishedResponses[1].ExtractedText, Is.EqualTo(recognizedText));
    }

    [Test]
    public async Task Consume_FileNotFound_PublishesFailed_AndThrows()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var message = new ProcessOcrDocument
        {
            JobId = jobId,
            DocumentId = documentId,
            DocumentName = "missing.pdf"
        };
        _consumeContextMock.Setup(x => x.Message).Returns(message);
        _consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _fileStorageMock
            .Setup(x => x.DownloadAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredFile?)null);

        var publishedResponses = new List<OcrDocumentResponse>();
        _consumeContextMock
            .Setup(x => x.Publish(It.IsAny<OcrDocumentResponse>(), It.IsAny<CancellationToken>()))
            .Callback<OcrDocumentResponse, CancellationToken>((resp, _) => publishedResponses.Add(resp))
            .Returns(Task.CompletedTask);

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _consumer.Consume(_consumeContextMock.Object));

        Assert.That(publishedResponses.Count, Is.EqualTo(2));
        Assert.That(publishedResponses[0].Status, Is.EqualTo(OcrStatus.Processing));
        Assert.That(publishedResponses[1].Status, Is.EqualTo(OcrStatus.Failed));
        _searchIndexMock.Verify(x => x.IndexAsync(It.IsAny<SearchDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Consume_OcrFails_PublishesFailed_AndThrows()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var message = new ProcessOcrDocument
        {
            JobId = jobId,
            DocumentId = documentId,
            DocumentName = "corrupt.pdf"
        };
        _consumeContextMock.Setup(x => x.Message).Returns(message);
        _consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var fileBytes = Encoding.UTF8.GetBytes("bad content");
        var memoryStream = new MemoryStream(fileBytes);
        var storedFile = new StoredFile(memoryStream, "application/pdf", fileBytes.Length);

        _fileStorageMock
            .Setup(x => x.DownloadAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedFile);

        _ocrServiceMock
            .Setup(x => x.ProcessPdfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ghostscript failed"));

        var publishedResponses = new List<OcrDocumentResponse>();
        _consumeContextMock
            .Setup(x => x.Publish(It.IsAny<OcrDocumentResponse>(), It.IsAny<CancellationToken>()))
            .Callback<OcrDocumentResponse, CancellationToken>((resp, _) => publishedResponses.Add(resp))
            .Returns(Task.CompletedTask);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _consumer.Consume(_consumeContextMock.Object));

        Assert.That(publishedResponses.Count, Is.EqualTo(2));
        Assert.That(publishedResponses[0].Status, Is.EqualTo(OcrStatus.Processing));
        Assert.That(publishedResponses[1].Status, Is.EqualTo(OcrStatus.Failed));
        _searchIndexMock.Verify(x => x.IndexAsync(It.IsAny<SearchDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
