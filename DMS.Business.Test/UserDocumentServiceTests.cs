using DMS.Business.Services;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;
using Moq;

namespace DMS.Business.Test;

public class UserDocumentServiceTests
{
    private Mock<DmsDbContext> _mockContext = null!;
    private Mock<DbSet<UserDocument>> _mockUserDocuments = null!;

    private User _max = null!;
    private User _anna = null!;
    private Document _document = null!;
    private UserDocument _link = null!;

    [SetUp]
    public void Setup()
    {
        _max = new User { Id = 1, EntityId = Guid.NewGuid() };
        _anna = new User { Id = 2, EntityId = Guid.NewGuid() };
        _document = new Document { Id = 10, EntityId = Guid.NewGuid() };
        _link = new UserDocument { UserId = 1, User = _max, DocumentId = 10, Document = _document, Permission = Permission.User };

        _mockUserDocuments = new List<UserDocument> { _link }.BuildMockDbSet();

        _mockContext = new Mock<DmsDbContext>(new DbContextOptions<DmsDbContext>());
        _mockContext.Setup(c => c.Set<User>()).Returns(new List<User> { _max, _anna }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Set<Document>()).Returns(new List<Document> { _document }.BuildMockDbSet().Object);
        _mockContext.Setup(c => c.Set<UserDocument>()).Returns(_mockUserDocuments.Object);
    }

    private UserDocumentService CreateService() => new(_mockContext.Object, NullLogger<UserDocumentService>.Instance);

    [Test]
    public async Task GrantAsync_NewLink_AddsWithoutSaving()
    {
        var service = CreateService();

        var result = await service.GrantAsync(_anna.EntityId, _document.EntityId, Permission.User);

        Assert.That(result.UserId, Is.EqualTo(2));
        Assert.That(result.DocumentId, Is.EqualTo(10));
        _mockUserDocuments.Verify(s => s.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void GrantAsync_ExistingLink_ThrowsArgumentException()
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() => service.GrantAsync(_max.EntityId, _document.EntityId, Permission.User));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void GrantAsync_UnknownUser_ThrowsArgumentException()
    {
        var service = CreateService();

        Assert.ThrowsAsync<ArgumentException>(() => service.GrantAsync(Guid.NewGuid(), _document.EntityId, Permission.User));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdatePermissionAsync_ExistingLink_ChangesPermission()
    {
        var service = CreateService();

        var result = await service.UpdatePermissionAsync(_max.EntityId, _document.EntityId, Permission.Owner);

        Assert.That(result, Is.True);
        Assert.That(_link.Permission, Is.EqualTo(Permission.Owner));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RevokeAsync_UnknownLink_ReturnsFalse()
    {
        var service = CreateService();

        var result = await service.RevokeAsync(_anna.EntityId, _document.EntityId);

        Assert.That(result, Is.False);
        _mockUserDocuments.Verify(s => s.Remove(It.IsAny<UserDocument>()), Times.Never);
    }
}
