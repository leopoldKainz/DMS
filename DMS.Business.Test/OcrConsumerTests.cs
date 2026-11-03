using DMS.Business.Models;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace DMS.Business.Test;

public class OcrConsumerTests
{
    private Mock<ILogger<OcrWorkerConsumer>> _mockWorkerLogger = null!;
    private Mock<ILogger<OcrResponseConsumer>> _mockResponseLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWorkerLogger = new Mock<ILogger<OcrWorkerConsumer>>();
        _mockResponseLogger = new Mock<ILogger<OcrResponseConsumer>>();
    }

    [Test]
    public async Task OcrWorkerConsumer_ConsumesRequestAndPublishesResponse()
    {
        // Arrange
        var consumer = new OcrWorkerConsumer(_mockWorkerLogger.Object);
        var jobId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var request = new ProcessOcrDocument { JobId = jobId, DocumentId = docId };

        var contextMock = new Mock<ConsumeContext<ProcessOcrDocument>>();
        contextMock.SetupGet(c => c.Message).Returns(request);

        ProcessOcrDocumentResponse? publishedResponse = null;
        contextMock.Setup(c => c.Publish(It.IsAny<ProcessOcrDocumentResponse>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((res, _) => publishedResponse = res as ProcessOcrDocumentResponse)
            .Returns(Task.CompletedTask);

        // Act
        await consumer.Consume(contextMock.Object);

        // Assert
        Assert.That(publishedResponse, Is.Not.Null);
        Assert.That(publishedResponse!.JobId, Is.EqualTo(jobId));
        Assert.That(publishedResponse.DocumentId, Is.EqualTo(docId));
        Assert.That(publishedResponse.Success, Is.True);
        Assert.That(publishedResponse.ExtractedText, Is.Not.Empty);
        contextMock.Verify(c => c.Publish(It.IsAny<ProcessOcrDocumentResponse>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task OcrResponseConsumer_ConsumesResponseSuccessfully()
    {
        // Arrange
        var consumer = new OcrResponseConsumer(_mockResponseLogger.Object);
        var jobId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var response = new ProcessOcrDocumentResponse
        {
            JobId = jobId,
            DocumentId = docId,
            Success = true,
            ExtractedText = "Test OCR content",
            ProcessedAt = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<ProcessOcrDocumentResponse>>();
        contextMock.SetupGet(c => c.Message).Returns(response);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await consumer.Consume(contextMock.Object));
    }
}
