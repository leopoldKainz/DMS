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

    // --- GetById ---

    [Test]
    public async Task GetById_ExistingId_ReturnsOkWithDocument()
    {
        // Arrange
        var docGuid = Guid.NewGuid();
        var document = new Document { Id = 1, EntityId = docGuid, Name = "invoice.pdf", Type = "application/pdf", Size = 100 };
        _mockService.Setup(s => s.GetByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act
        var result = await _controller.GetById(docGuid);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as DocumentDto;
        Assert.That(dto!.Id, Is.EqualTo(docGuid));
        Assert.That(dto.Name, Is.EqualTo("invoice.pdf"));
    }

    [Test]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        // Arrange
        var unknownGuid = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(unknownGuid, It.IsAny<CancellationToken>())).ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.GetById(unknownGuid);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    // --- UploadMultipleFiles ---

    [Test]
    public async Task Upload_ValidFile_SavesDocumentForOwner()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var docGuid = Guid.NewGuid();
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("invoice.pdf");
        file.Setup(f => f.Length).Returns(100);
        file.Setup(f => f.ContentType).Returns("application/pdf");
        _mockService.Setup(s => s.AddForOwnerAsync(userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => { d.EntityId = docGuid; return d; });

        // Act
        var result = await _controller.UploadMultipleFiles(new List<IFormFile> { file.Object }, userGuid);

        // Assert
        var ok = result.Result as OkObjectResult;
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
        Assert.That(dtos![0].Id, Is.EqualTo(docGuid));
        Assert.That(dtos[0].Name, Is.EqualTo("invoice"));
        Assert.That(dtos[0].Type, Is.EqualTo("pdf"));
        _mockService.Verify(s => s.AddForOwnerAsync(userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Upload_NoFiles_ReturnsBadRequest()
    {
        // Act
        var userGuid = Guid.NewGuid();
        var result = await _controller.UploadMultipleFiles(new List<IFormFile>(), userGuid);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // --- GetAll ---

    [Test]
    public async Task GetAll_WithUserId_ReturnsOkWithUserDocuments()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var docGuid = Guid.NewGuid();
        var docs = new List<Document>
        {
            new() { Id = 1, EntityId = docGuid, Name = "contract.pdf", Type = "application/pdf", Size = 200 }
        };
        _mockService.Setup(s => s.GetByUserAsync(userGuid, It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        // Act
        var result = await _controller.GetAll(userGuid);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
        Assert.That(dtos![0].Id, Is.EqualTo(docGuid));
    }

    [Test]
    public async Task GetAll_WithoutUserId_ReturnsAllDocuments()
    {
        // Arrange
        var docs = new List<Document>
        {
            new() { Id = 1, EntityId = Guid.NewGuid(), Name = "doc1.pdf", Type = "application/pdf", Size = 50 },
            new() { Id = 2, EntityId = Guid.NewGuid(), Name = "doc2.pdf", Type = "application/pdf", Size = 80 }
        };
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        // Act
        var result = await _controller.GetAll(null);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(2));
    }

    // --- Search ---

    [Test]
    public async Task Search_MatchingTerm_ReturnsOkWithDocuments()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var docs = new List<Document>
        {
            new() { Id = 1, EntityId = Guid.NewGuid(), Name = "matching_doc.pdf", Type = "application/pdf", Size = 120 }
        };
        _mockService.Setup(s => s.SearchByNameAsync(userGuid, "matching", It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        // Act
        var result = await _controller.Search("matching", userGuid);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_NonMatchingTerm_ReturnsOkWithEmptyList()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        _mockService.Setup(s => s.SearchByNameAsync(userGuid, "nonexistent", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Document>());

        // Act
        var result = await _controller.Search("nonexistent", userGuid);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Is.Empty);
    }

    [Test]
    public async Task Search_EmptyQuery_DelegatesToGetAll()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        _mockService.Setup(s => s.GetByUserAsync(userGuid, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Document>());

        // Act
        var result = await _controller.Search("", userGuid);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
    }

    [Test]
    public async Task Search_WithoutUserId_FiltersAllDocuments()
    {
        // Arrange
        var docs = new List<Document>
        {
            new() { Id = 1, EntityId = Guid.NewGuid(), Name = "Invoice_2026.pdf", Type = "application/pdf", Size = 100 },
            new() { Id = 2, EntityId = Guid.NewGuid(), Name = "Contract.pdf", Type = "application/pdf", Size = 100 }
        };
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        // Act
        var result = await _controller.Search("invoice", null);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
    }

    // --- Delete ---

    [Test]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var docGuid = Guid.NewGuid();
        _mockService.Setup(s => s.RemoveByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(docGuid);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        // Arrange
        var unknownGuid = Guid.NewGuid();
        _mockService.Setup(s => s.RemoveByIdAsync(unknownGuid, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(unknownGuid);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
