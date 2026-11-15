using System.Security.Claims;
using DMS.API.Controllers;
using DMS.Business.Exceptions;
using DMS.Business.Interfaces;
using DMS.Business.Models;
using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using DMS.ELK.Exceptions;
using DMS.ELK.Interfaces;
using DMS.Storage;
using DMS.Storage.Exceptions;
using DMS.Storage.Interfaces;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace DMS.API.Test;

public class DocumentControllerTests
{
    private Mock<IDocumentService> _mockService = null!;
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private Mock<IPublishEndpoint> _mockPublishEndpoint = null!;
    private Mock<IFileStorage> _mockFileStorage = null!;
    private Mock<ISearchIndex> _mockSearchIndex = null!;
    private DocumentController _controller = null!;
    private Guid _userGuid;

    [OneTimeSetUp]
    public void RegisterMappings()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
    }

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IDocumentService>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _userGuid = Guid.NewGuid();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockFileStorage = new Mock<IFileStorage>();
        _mockSearchIndex = new Mock<ISearchIndex>();
        _controller = new DocumentController(
            _mockService.Object,
            _mockUnitOfWork.Object,
            _mockPublishEndpoint.Object,
            _mockFileStorage.Object,
            _mockSearchIndex.Object,
            NullLogger<DocumentController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(JwtRegisteredClaimNames.Sub, _userGuid.ToString())], "Test"))
                }
            }
        };
    }

    // --- GetById ---

    [Test]
    public async Task GetById_ExistingId_ReturnsOkWithDocument()
    {
        // Arrange
        var docGuid = Guid.NewGuid();
        var document = new Document { Id = 1, EntityId = docGuid, Name = "invoice.pdf", Type = "application/pdf", Size = 100 };
        _mockService.Setup(s => s.GetByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);

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

    [Test]
    public async Task GetById_DocumentOfOtherUser_ReturnsNotFound()
    {
        var docGuid = Guid.NewGuid();
        var document = new Document { Id = 1, EntityId = docGuid, Name = "foreign", Type = "pdf", Size = 100 };
        _mockService.Setup(s => s.GetByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.GetById(docGuid);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    // --- UploadMultipleFiles ---

    [Test]
    public async Task Upload_ValidFile_SavesDocumentForOwner()
    {
        // Arrange
        var docGuid = Guid.NewGuid();
        var file = CreateFormFile("invoice.pdf", "application/pdf");
        _mockService.Setup(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => { d.EntityId = docGuid; return d; });

        // Act
        var result = await _controller.UploadMultipleFiles(new List<IFormFile> { file.Object });

        // Assert
        var ok = result.Result as OkObjectResult;
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
        Assert.That(dtos![0].Id, Is.EqualTo(docGuid));
        Assert.That(dtos[0].Name, Is.EqualTo("invoice"));
        Assert.That(dtos[0].Type, Is.EqualTo("pdf"));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockService.Verify(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Upload_ValidFile_StoresFileUnderDocumentId()
    {
        var docGuid = Guid.NewGuid();
        var file = CreateFormFile("invoice.pdf", "application/pdf");
        _mockService.Setup(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => { d.EntityId = docGuid; return d; });

        await _controller.UploadMultipleFiles(new List<IFormFile> { file.Object });

        _mockFileStorage.Verify(s => s.UploadAsync(docGuid, It.IsAny<Stream>(), 100, "application/pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Upload_MissingContentType_StoresFileAsOctetStream()
    {
        var file = CreateFormFile("notes.txt");
        _mockService.Setup(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => d);

        await _controller.UploadMultipleFiles(new List<IFormFile> { file.Object });

        _mockFileStorage.Verify(s => s.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), 100, "application/octet-stream", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Upload_StorageUnavailable_ThrowsStorageExceptionAndDoesNotSave()
    {
        var file = CreateFormFile("invoice.pdf", "application/pdf");
        _mockService.Setup(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => d);
        _mockFileStorage.Setup(s => s.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new StorageException("RustFS is down", new InvalidOperationException()));

        Assert.ThrowsAsync<StorageException>(() => _controller.UploadMultipleFiles(new List<IFormFile> { file.Object }));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProcessOcrDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Upload_SaveFails_RemovesStoredFiles()
    {
        var docGuid = Guid.NewGuid();
        var file = CreateFormFile("invoice.pdf", "application/pdf");
        _mockService.Setup(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => { d.EntityId = docGuid; return d; });
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("Database is down"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UploadMultipleFiles(new List<IFormFile> { file.Object }));
        _mockFileStorage.Verify(s => s.DeleteAsync(docGuid, It.IsAny<CancellationToken>()), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProcessOcrDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Upload_NoFiles_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UploadMultipleFiles(new List<IFormFile>());

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void Upload_QueueUnavailable_SavesDocumentAndThrowsMessagingException()
    {
        // Arrange
        var file = CreateFormFile("invoice.pdf");
        _mockService.Setup(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => d);
        _mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<ProcessOcrDocument>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(new InvalidOperationException("RabbitMQ is down"));

        // Act + Assert
        var ex = Assert.ThrowsAsync<MessagingException>(() => _controller.UploadMultipleFiles(new List<IFormFile> { file.Object }));
        Assert.That(ex!.InnerException, Is.InstanceOf<InvalidOperationException>());
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Upload_ValidFile_StoresContentTypeInMetadataWithPendingOcr()
    {
        var file = CreateFormFile("invoice.pdf", "application/pdf");
        Document? saved = null;
        _mockService.Setup(s => s.AddForOwnerAsync(_userGuid, It.IsAny<Document>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Guid _, Document d, CancellationToken _) => { saved = d; return d; });

        var result = await _controller.UploadMultipleFiles(new List<IFormFile> { file.Object });

        Assert.That(saved!.Metadata.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(saved.OcrStatus, Is.EqualTo(OcrStatus.Pending));
        var dtos = (result.Result as OkObjectResult)?.Value as List<DocumentDto>;
        Assert.That(dtos![0].Metadata.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(dtos[0].OcrStatus, Is.EqualTo("Pending"));
    }

    [Test]
    public async Task GetUsers_DocumentWithShares_ReturnsOwnerFirst()
    {
        var docGuid = Guid.NewGuid();
        var owner = new User { EntityId = Guid.NewGuid(), Username = "owner" };
        var reader = new User { EntityId = Guid.NewGuid(), Username = "reader" };
        var document = new Document { Id = 1, EntityId = docGuid, Name = "invoice", Type = "pdf", Size = 3 };
        document.UserDocuments.Add(new UserDocument { User = reader, Document = document, Permission = Permission.User, CreatedAt = DateTime.UtcNow });
        document.UserDocuments.Add(new UserDocument { User = owner, Document = document, Permission = Permission.Owner, CreatedAt = DateTime.UtcNow.AddDays(-1) });
        _mockService.Setup(s => s.GetWithUsersAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _controller.GetUsers(docGuid);

        var users = (result.Result as OkObjectResult)?.Value as List<UserDocumentDto>;
        Assert.That(users, Has.Count.EqualTo(2));
        Assert.That(users![0].Username, Is.EqualTo("owner"));
        Assert.That(users[0].Permission, Is.EqualTo("Owner"));
        Assert.That(users[1].Username, Is.EqualTo("reader"));
        Assert.That(users[1].Permission, Is.EqualTo("User"));
    }

    [Test]
    public async Task GetUsers_DocumentOfOtherUser_ReturnsNotFound()
    {
        var docGuid = Guid.NewGuid();
        var document = new Document { Id = 1, EntityId = docGuid, Name = "foreign", Type = "pdf", Size = 3 };
        _mockService.Setup(s => s.GetWithUsersAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.GetUsers(docGuid);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Download_ExistingFile_ReturnsFileWithOriginalName()
    {
        var docGuid = Guid.NewGuid();
        var document = new Document { Id = 1, EntityId = docGuid, Name = "invoice", Type = "pdf", Size = 3 };
        _mockService.Setup(s => s.GetByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockFileStorage.Setup(s => s.DownloadAsync(docGuid, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new StoredFile(new MemoryStream([1, 2, 3]), "application/pdf", 3));

        var result = await _controller.Download(docGuid);

        var fileResult = result as FileStreamResult;
        Assert.That(fileResult, Is.Not.Null);
        Assert.That(fileResult!.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(fileResult.FileDownloadName, Is.EqualTo("invoice.pdf"));
    }

    [Test]
    public async Task Download_DocumentOfOtherUser_ReturnsNotFound()
    {
        var docGuid = Guid.NewGuid();
        var document = new Document { Id = 1, EntityId = docGuid, Name = "foreign", Type = "pdf", Size = 3 };
        _mockService.Setup(s => s.GetByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.Download(docGuid);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        _mockFileStorage.Verify(s => s.DownloadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Download_FileMissingInStorage_ReturnsNotFound()
    {
        var docGuid = Guid.NewGuid();
        var document = new Document { Id = 1, EntityId = docGuid, Name = "invoice", Type = "pdf", Size = 3 };
        _mockService.Setup(s => s.GetByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockFileStorage.Setup(s => s.DownloadAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync((StoredFile?)null);

        var result = await _controller.Download(docGuid);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    // --- GetAll ---

    [Test]
    public async Task GetAll_ReturnsOkWithDocumentsOfCurrentUser()
    {
        // Arrange
        var docGuid = Guid.NewGuid();
        var docs = new List<Document>
        {
            new() { Id = 1, EntityId = docGuid, Name = "contract.pdf", Type = "application/pdf", Size = 200 }
        };
        _mockService.Setup(s => s.GetByUserAsync(_userGuid, It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
        Assert.That(dtos![0].Id, Is.EqualTo(docGuid));
    }

    // --- Search ---

    [Test]
    public async Task Search_MatchingTerm_PassesIndexHitsToServiceAndReturnsOk()
    {
        // Arrange: Elasticsearch finds the term inside the document, the service resolves it for the user
        var docGuid = Guid.NewGuid();
        var docs = new List<Document>
        {
            new() { Id = 1, EntityId = docGuid, Name = "matching_doc.pdf", Type = "application/pdf", Size = 120 }
        };
        _mockSearchIndex.Setup(s => s.SearchAsync("matching", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Guid> { docGuid });
        _mockService.Setup(s => s.SearchAsync(_userGuid, "matching",
                                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(docGuid)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(docs);

        // Act
        var result = await _controller.Search("matching");

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
        Assert.That(dtos![0].Id, Is.EqualTo(docGuid));
        _mockSearchIndex.Verify(s => s.SearchAsync("matching", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Search_NonMatchingTerm_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockSearchIndex.Setup(s => s.SearchAsync("nonexistent", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Guid>());
        _mockService.Setup(s => s.SearchAsync(_userGuid, "nonexistent", It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Document>());

        // Act
        var result = await _controller.Search("nonexistent");

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Is.Empty);
    }

    [Test]
    public async Task Search_IndexUnavailable_FallsBackToNameSearch()
    {
        // Arrange: Elasticsearch is down, the search must still work on document names
        var docs = new List<Document>
        {
            new() { Id = 1, EntityId = Guid.NewGuid(), Name = "invoice.pdf", Type = "application/pdf", Size = 120 }
        };
        _mockSearchIndex.Setup(s => s.SearchAsync("invoice", It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new SearchIndexException("Elasticsearch is down"));
        _mockService.Setup(s => s.SearchAsync(_userGuid, "invoice",
                                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 0), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(docs);

        // Act
        var result = await _controller.Search("invoice");

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<DocumentDto>;
        Assert.That(dtos, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_EmptyQuery_DelegatesToGetAll()
    {
        // Arrange
        _mockService.Setup(s => s.GetByUserAsync(_userGuid, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Document>());

        // Act
        var result = await _controller.Search("");

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
    }

    // --- Delete ---

    [Test]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var docGuid = Guid.NewGuid();
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockService.Setup(s => s.HasPermissionAsync(_userGuid, docGuid, Permission.Owner, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockService.Setup(s => s.RemoveByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(docGuid);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockFileStorage.Verify(s => s.DeleteAsync(docGuid, It.IsAny<CancellationToken>()), Times.Once);
        _mockSearchIndex.Verify(s => s.DeleteAsync(docGuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delete_StorageAndIndexUnavailable_StillReturnsNoContent()
    {
        var docGuid = Guid.NewGuid();
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockService.Setup(s => s.HasPermissionAsync(_userGuid, docGuid, Permission.Owner, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockService.Setup(s => s.RemoveByIdAsync(docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockFileStorage.Setup(s => s.DeleteAsync(docGuid, It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new StorageException("RustFS is down", new InvalidOperationException()));
        _mockSearchIndex.Setup(s => s.DeleteAsync(docGuid, It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new SearchIndexException("Elasticsearch is down"));

        var result = await _controller.Delete(docGuid);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        // Arrange
        var unknownGuid = Guid.NewGuid();
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, unknownGuid, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(unknownGuid);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        _mockService.Verify(s => s.RemoveByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Delete_SharedButNotOwner_ReturnsForbidden()
    {
        var docGuid = Guid.NewGuid();
        _mockService.Setup(s => s.HasAccessAsync(_userGuid, docGuid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockService.Setup(s => s.HasPermissionAsync(_userGuid, docGuid, Permission.Owner, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.Delete(docGuid);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        _mockService.Verify(s => s.RemoveByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<IFormFile> CreateFormFile(string fileName, string contentType = "")
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(100);
        file.Setup(f => f.ContentType).Returns(contentType);
        file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(new byte[100]));
        return file;
    }
}
