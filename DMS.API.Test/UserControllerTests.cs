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
        var dto = new CreateUserDto("max", "max@test.at", "secret");
        _mockService.Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User u, CancellationToken _) => { u.Id = 1; return u; });

        // Act
        var result = await _controller.PostRegister(dto);

        // Assert
        var created = result.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        var userDto = created!.Value as UserDto;
        Assert.That(userDto!.Id, Is.EqualTo(1));
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
        var user = new User { Id = 1, Username = "max", Email = "max@test.at" };
        _mockService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as UserDto;
        Assert.That(dto!.Username, Is.EqualTo("max"));
    }

    [Test]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetById(99);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
