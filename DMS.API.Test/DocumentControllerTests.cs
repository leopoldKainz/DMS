using DMS.API.Controllers;
using DMS.Business.Interfaces;
using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DMS.API.Test;

public class DocumentControllerTests
{
    private Mock<IDocumentService> _mockService = null!;
    private DocumentController _controller = null!;

    [OneTimeSetUp]
    public void RegisterMappings()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
    }

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IDocumentService>();
        _controller = new DocumentController(_mockService.Object);
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOkWithDocument()
    {
        // Arrange
        var document = new Document { Id = 1, Name = "invoice.pdf", Type = "application/pdf", Size = 100 };
        _mockService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as DocumentDto;
        Assert.That(dto!.Id, Is.EqualTo(1));
        Assert.That(dto.Name, Is.EqualTo("invoice.pdf"));
    }

    [Test]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.GetById(99);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Upload_NoFiles_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UploadMultipleFiles(new List<IFormFile>(), 1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Upload_ValidFile_SavesDocumentForOwner()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("invoice.pdf");
        file.Setup(f => f.Length).Returns(100);
        file.Setup(f => f.ContentType).Returns("application/pdf");
        _mockService.Setup(s => s.AddForOwnerAsync(1, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((int _, Document d, CancellationToken _) => d);

        // Act
        var result = await _controller.UploadMultipleFiles(new List<IFormFile> { file.Object }, 1);

        // Assert
        var ok = result.Result as OkObjectResult;
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
        Assert.That(dtos![0].Name, Is.EqualTo("invoice"));
        Assert.That(dtos[0].Type, Is.EqualTo("pdf"));
        _mockService.Verify(s => s.AddForOwnerAsync(1, It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.RemoveByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(99);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
