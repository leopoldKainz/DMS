using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Mapster;
using NUnit.Framework;

namespace DMS.Business.Test.MappingTests;

[TestFixture]
public class MappingConfigTests
{
    private TypeAdapterConfig _config = null!;

    [SetUp]
    public void SetUp()
    {
        _config = new TypeAdapterConfig();
        new MappingConfig().Register(_config);
    }

    [Test]
    public void User_To_UserDto_MapsCorrectly()
    {
        var entityId = Guid.NewGuid();
        var user = new User
        {
            Id = 1,
            EntityId = entityId,
            Username = "johndoe",
            Email = "john@example.com",
            CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var dto = user.Adapt<UserDto>(_config);

        Assert.That(dto.Id, Is.EqualTo(entityId));
        Assert.That(dto.Username, Is.EqualTo("johndoe"));
        Assert.That(dto.Email, Is.EqualTo("john@example.com"));
        Assert.That(dto.CreatedAt, Is.EqualTo(user.CreatedAt));
    }

    [Test]
    public void CreateUserDto_To_User_MapsAndIgnoresCorrectly()
    {
        var dto = new CreateUserDto("janedoe", "jane@example.com", "SecurePass123!");

        var user = dto.Adapt<User>(_config);

        Assert.That(user.Username, Is.EqualTo("janedoe"));
        Assert.That(user.Email, Is.EqualTo("jane@example.com"));
        Assert.That(user.Password, Is.EqualTo("SecurePass123!"));
        Assert.That(user.Id, Is.EqualTo(0));
        Assert.That(user.CreatedAt, Is.EqualTo(default(DateTime)));
        Assert.That(user.UpdatedOn, Is.Null);
    }

    [Test]
    public void Document_To_DocumentDto_MapsCorrectly()
    {
        var entityId = Guid.NewGuid();
        var doc = new Document
        {
            Id = 42,
            EntityId = entityId,
            Name = "tax_return.pdf",
            Type = "application/pdf",
            Size = 2048,
            CreatedAt = new DateTime(2025, 2, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var dto = doc.Adapt<DocumentDto>(_config);

        Assert.That(dto.Id, Is.EqualTo(entityId));
        Assert.That(dto.Name, Is.EqualTo("tax_return.pdf"));
        Assert.That(dto.Type, Is.EqualTo("application/pdf"));
        Assert.That(dto.Size, Is.EqualTo(2048));
        Assert.That(dto.CreatedAt, Is.EqualTo(doc.CreatedAt));
    }

    [Test]
    public void UserDocument_To_UserDocumentDto_WithNavigationProperties_MapsCorrectly()
    {
        var userId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        var userDocument = new UserDocument
        {
            UserId = 1,
            User = new User { Id = 1, EntityId = userId, Username = "admin" },
            DocumentId = 10,
            Document = new Document { Id = 10, EntityId = docId, Name = "contract.pdf" },
            Permission = Permission.Owner,
            CreatedAt = new DateTime(2025, 3, 1, 8, 30, 0, DateTimeKind.Utc)
        };

        var dto = userDocument.Adapt<UserDocumentDto>(_config);

        Assert.That(dto.UserId, Is.EqualTo(userId));
        Assert.That(dto.Username, Is.EqualTo("admin"));
        Assert.That(dto.DocumentId, Is.EqualTo(docId));
        Assert.That(dto.DocumentName, Is.EqualTo("contract.pdf"));
        Assert.That(dto.Permission, Is.EqualTo("Owner"));
        Assert.That(dto.CreatedAt, Is.EqualTo(userDocument.CreatedAt));
    }

    [Test]
    public void UserDocument_To_UserDocumentDto_NullNavigationProperties_MapsDefaults()
    {
        var userDocument = new UserDocument
        {
            UserId = 1,
            User = null!,
            DocumentId = 10,
            Document = null!,
            Permission = Permission.User,
            CreatedAt = new DateTime(2025, 3, 1, 8, 30, 0, DateTimeKind.Utc)
        };

        var dto = userDocument.Adapt<UserDocumentDto>(_config);

        Assert.That(dto.UserId, Is.EqualTo(Guid.Empty));
        Assert.That(dto.Username, Is.Null);
        Assert.That(dto.DocumentId, Is.EqualTo(Guid.Empty));
        Assert.That(dto.DocumentName, Is.Null);
        Assert.That(dto.Permission, Is.EqualTo("User"));
    }

    [Test]
    public void ShareRequest_To_ShareRequestDto_MapsCorrectly()
    {
        var requestId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        var shareRequest = new ShareRequest
        {
            Id = 5,
            EntityId = requestId,
            Owner = new User { Id = 1, EntityId = ownerId, Username = "ownerUser" },
            Reciever = new User { Id = 2, EntityId = receiverId, Username = "receiverUser" },
            Document = new Document { Id = 10, EntityId = documentId, Name = "shared.pdf" },
            CreatedAt = new DateTime(2025, 4, 1, 9, 0, 0, DateTimeKind.Utc)
        };

        var dto = shareRequest.Adapt<ShareRequestDto>(_config);

        Assert.That(dto.Id, Is.EqualTo(requestId));
        Assert.That(dto.OwnerId, Is.EqualTo(ownerId));
        Assert.That(dto.RecieverId, Is.EqualTo(receiverId));
        Assert.That(dto.DocumentId, Is.EqualTo(documentId));
        Assert.That(dto.CreatedAt, Is.EqualTo(shareRequest.CreatedAt));
    }
}
