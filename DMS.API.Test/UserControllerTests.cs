using System.Security.Claims;
using DMS.API.Auth;
using DMS.API.Controllers;
using DMS.Business.Interfaces;
using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace DMS.API.Test;

public class UserControllerTests
{
    private Mock<IUserService> _mockService = null!;
    private Mock<ITokenService> _mockTokenService = null!;
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private UserController _controller = null!;
    private Guid _currentUserGuid;

    [OneTimeSetUp]
    public void RegisterMappings()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfig).Assembly);
    }

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IUserService>();
        _mockTokenService = new Mock<ITokenService>();
        _currentUserGuid = Guid.NewGuid();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new UserController(_mockService.Object, _mockTokenService.Object, _mockUnitOfWork.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(JwtRegisteredClaimNames.Sub, _currentUserGuid.ToString())], "Test"))
                }
            }
        };
    }

    [Test]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var dto = new CreateUserDto("max", "max@test.at", "secret");
        _mockService.Setup(s => s.UsernameExistsAsync("max", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockService.Setup(s => s.EmailExistsAsync("max@test.at", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockService.Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User u, CancellationToken _) => { u.EntityId = userGuid; return u; });

        // Act
        var result = await _controller.PostRegister(dto);

        // Assert
        var created = result.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        var createdDto = created!.Value as UserDto;
        Assert.That(createdDto!.Id, Is.EqualTo(userGuid));
        Assert.That(createdDto.Username, Is.EqualTo("max"));
        Assert.That(createdDto.Email, Is.EqualTo("max@test.at"));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Register_EmailTaken_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateUserDto("max", "max@test.at", "secret");
        _mockService.Setup(s => s.UsernameExistsAsync("max", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockService.Setup(s => s.EmailExistsAsync("max@test.at", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.PostRegister(dto);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithTokenAndUser()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var user = new User { Id = 1, EntityId = userGuid, Username = "max", Email = "max@test.at" };
        var dto = new LoginDto("max", "secret");
        var expiresAt = DateTime.UtcNow.AddHours(1);
        _mockService.Setup(s => s.AuthenticateAsync("max", "secret", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockTokenService.Setup(t => t.CreateToken(user)).Returns(("jwt-token", expiresAt));

        // Act
        var result = await _controller.PostLogin(dto);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var response = ok!.Value as LoginResponseDto;
        Assert.That(response!.Token, Is.EqualTo("jwt-token"));
        Assert.That(response.ExpiresAt, Is.EqualTo(expiresAt));
        Assert.That(response.User.Id, Is.EqualTo(userGuid));
        Assert.That(response.User.Username, Is.EqualTo("max"));
    }

    [Test]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new LoginDto("max", "wrong");
        _mockService.Setup(s => s.AuthenticateAsync("max", "wrong", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.PostLogin(dto);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_MissingCredentials_ReturnsBadRequest()
    {
        // Arrange
        var dto = new LoginDto("", "");

        // Act
        var result = await _controller.PostLogin(dto);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
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
        Assert.That(ok, Is.Not.Null);
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

    [Test]
    public async Task GetAll_UsersExist_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, EntityId = Guid.NewGuid(), Username = "user1", Email = "u1@test.at" },
            new() { Id = 2, EntityId = Guid.NewGuid(), Username = "user2", Email = "u2@test.at" }
        };
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<UserDto>;
        Assert.That(dtos, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAll_NoUsers_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dtos = ok!.Value as List<UserDto>;
        Assert.That(dtos, Is.Empty);
    }

    [Test]
    public async Task GetByUsername_ExistingUsername_ReturnsOkWithUser()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var user = new User { Id = 1, EntityId = userGuid, Username = "existing", Email = "e@test.at" };
        _mockService.Setup(s => s.GetByUsernameAsync("existing", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _controller.GetByUsername("existing");

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as UserDto;
        Assert.That(dto!.Id, Is.EqualTo(userGuid));
        Assert.That(dto.Username, Is.EqualTo("existing"));
    }

    [Test]
    public async Task GetByUsername_UnknownUsername_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetByUsername("unknown");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetByEmail_ExistingEmail_ReturnsOkWithUser()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var user = new User { Id = 1, EntityId = userGuid, Username = "max", Email = "max@test.at" };
        _mockService.Setup(s => s.GetByEmailAsync("max@test.at", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _controller.GetByEmail("max@test.at");

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as UserDto;
        Assert.That(dto!.Id, Is.EqualTo(userGuid));
        Assert.That(dto.Email, Is.Null);
    }

    [Test]
    public async Task GetById_CurrentUser_ReturnsOwnEmail()
    {
        var user = new User { Id = 1, EntityId = _currentUserGuid, Username = "me", Email = "me@test.at" };
        _mockService.Setup(s => s.GetByIdAsync(_currentUserGuid, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _controller.GetById(_currentUserGuid);

        var dto = (result.Result as OkObjectResult)!.Value as UserDto;
        Assert.That(dto!.Email, Is.EqualTo("me@test.at"));
    }

    [Test]
    public async Task GetAll_OtherUsers_HidesTheirEmails()
    {
        var users = new List<User>
        {
            new() { Id = 1, EntityId = _currentUserGuid, Username = "me", Email = "me@test.at" },
            new() { Id = 2, EntityId = Guid.NewGuid(), Username = "other", Email = "other@test.at" }
        };
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var result = await _controller.GetAll();

        var dtos = (result.Result as OkObjectResult)!.Value as List<UserDto>;
        Assert.That(dtos![0].Email, Is.EqualTo("me@test.at"));
        Assert.That(dtos[1].Email, Is.Null);
    }

    [Test]
    public async Task GetMe_ExistingUser_ReturnsOwnUser()
    {
        var user = new User { Id = 1, EntityId = _currentUserGuid, Username = "me", Email = "me@test.at" };
        _mockService.Setup(s => s.GetByIdAsync(_currentUserGuid, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _controller.GetMe();

        var dto = (result.Result as OkObjectResult)!.Value as UserDto;
        Assert.That(dto!.Id, Is.EqualTo(_currentUserGuid));
    }

    [Test]
    public async Task GetMe_DeletedUser_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetByIdAsync(_currentUserGuid, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await _controller.GetMe();

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetByEmail_UnknownEmail_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByEmailAsync("unknown@test.at", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetByEmail("unknown@test.at");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
