using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;
using Moq;

namespace DMS.Business.Test;

public class ShareRequestServiceTests
{
    private Mock<DmsDbContext> _mockContext = null!;
    private Mock<DbSet<UserDocument>> _mockUserDocuments = null!;
    private Mock<DbSet<ShareRequest>> _mockShareRequests = null!;

    private User _owner = null!;
    private User _reciever = null!;
    private User _other = null!;
    private Document _document = null!;
    private ShareRequest _pendingRequest = null!;

    [SetUp]
    public void Setup()
    {
        _owner = new User { Id = 1, EntityId = Guid.NewGuid(), Username = "max" };
        _reciever = new User { Id = 2, EntityId = Guid.NewGuid(), Username = "anna" };
        _other = new User { Id = 3, EntityId = Guid.NewGuid(), Username = "tom" };
        _document = new Document { Id = 10, EntityId = Guid.NewGuid(), Name = "invoice.pdf" };
        _pendingRequest = new ShareRequest { Id = 5, EntityId = Guid.NewGuid(), Owner = _owner, Reciever = _other, Document = _document };

        var userDocuments = new List<UserDocument>
        {
            new() { UserId = 1, User = _owner, DocumentId = 10, Document = _document, Permission = Permission.Owner }
        };

        _mockUserDocuments = userDocuments.BuildMockDbSet();
        _mockShareRequests = new List<ShareRequest> { _pendingRequest }.BuildMockDbSet();

        _mockContext = new Mock<DmsDbContext>(new DbContextOptions<DmsDbContext>());
        _mockContext.Setup(c => c.Set<User>()).Returns(new List<User> { _owner, _reciever, _other }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Set<Document>()).Returns(new List<Document> { _document }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Set<UserDocument>()).Returns(_mockUserDocuments.Object);
        _mockContext.Setup(c => c.Set<ShareRequest>()).Returns(_mockShareRequests.Object);
    }

    private ShareRequestService CreateService() => new(_mockContext.Object, NullLogger<ShareRequestService>.Instance);

    [Test]
    public async Task CreateAsync_OwnerSharesDocument_AddsRequestWithoutSaving()
    {
        var service = CreateService();

        var result = await service.CreateAsync(_owner.EntityId, _reciever.EntityId, _document.EntityId);

        Assert.That(result.Owner, Is.SameAs(_owner));
        Assert.That(result.Reciever, Is.SameAs(_reciever));
        Assert.That(result.Document, Is.SameAs(_document));
        _mockShareRequests.Verify(s => s.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void CreateAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var service = CreateService();

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(_reciever.EntityId, _owner.EntityId, _document.EntityId));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void CreateAsync_DuplicateRequest_ThrowsArgumentException()
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(_owner.EntityId, _other.EntityId, _document.EntityId));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AcceptAsync_GrantsAccessAndRemovesRequest()
    {
        var service = CreateService();

        var result = await service.AcceptAsync(_pendingRequest.EntityId, Permission.User);

        Assert.That(result, Is.True);
        _mockUserDocuments.Verify(s => s.AddAsync(
            It.Is<UserDocument>(ud => ud.UserId == 3 && ud.DocumentId == 10 && ud.Permission == Permission.User),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockShareRequests.Verify(s => s.Remove(_pendingRequest), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AcceptAsync_UnknownId_ReturnsFalse()
    {
        var service = CreateService();

        var result = await service.AcceptAsync(Guid.NewGuid(), Permission.User);

        Assert.That(result, Is.False);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
