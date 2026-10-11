using DMS.API.Controllers;
using DMS.Business.Interfaces;
using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DMS.API.Test;

public class UserControllerTests
{
    private Mock<IUserService> _mockService = null!;
    private UserController _controller = null!;

    [OneTimeSetUp]
    public void RegisterMappings()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
    }

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UserController(_mockService.Object);
    }

    [Test]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var dto = new CreateUserDto("max", "max@test.at", "secret");
        _mockService.Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User u, CancellationToken _) => { u.EntityId = userGuid; return u; });

        // Act
        var result = await _controller.PostRegister(dto);

        // Assert
        var created = result.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        var userDto = created!.Value as UserDto;
        Assert.That(userDto!.Id, Is.EqualTo(userGuid));
        Assert.That(userDto.Username, Is.EqualTo("max"));
    }

    [Test]
    public async Task Register_UsernameTaken_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateUserDto("max", "max@test.at", "secret");
        _mockService.Setup(s => s.UsernameExistsAsync("max", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.PostRegister(dto);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        _mockService.Verify(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOkWithUser()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var user = new User { Id = 1, EntityId = userGuid, Username = "max", Email = "max@test.at" };
        _mockService.Setup(s => s.GetByIdAsync(userGuid, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _controller.GetById(userGuid);

        // Assert
        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as UserDto;
        Assert.That(dto!.Id, Is.EqualTo(userGuid));
        Assert.That(dto.Username, Is.EqualTo("max"));
    }

    [Test]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        // Arrange
        var unknownGuid = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(unknownGuid, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetById(unknownGuid);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
