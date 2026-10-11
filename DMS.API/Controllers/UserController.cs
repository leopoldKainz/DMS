using DMS.Business.Interfaces;
using DMS.DAL.DTOs;
using DMS.DAL.Entities;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> PostRegister([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Username, Email, and Password are required.");
        }

        if (await _userService.UsernameExistsAsync(dto.Username))
        {
            return BadRequest($"Username '{dto.Username}' is already taken.");
        }

        if (await _userService.EmailExistsAsync(dto.Email))
        {
            return BadRequest($"Email '{dto.Email}' is already registered.");
        }

        User user = dto.Adapt<User>();
        User createdUser = await _userService.AddAsync(user);
        UserDto resultDto = createdUser.Adapt<UserDto>();

        return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> PostLogin([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Username/Email and Password are required.");
        }

        User? user = await _userService.AuthenticateAsync(dto.UsernameOrEmail, dto.Password);
        if (user == null)
        {
            return Unauthorized("Invalid username/email or password.");
        }

        return Ok(user.Adapt<UserDto>());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        User? user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        return Ok(user.Adapt<UserDto>());
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        IReadOnlyList<User> users = await _userService.GetAllAsync();
        return Ok(users.Adapt<List<UserDto>>());
    }

    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<UserDto>> GetByUsername(string username)
    {
        User? user = await _userService.GetByUsernameAsync(username);
        if (user == null)
        {
            return NotFound($"User '{username}' not found.");
        }

        return Ok(user.Adapt<UserDto>());
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<UserDto>> GetByEmail(string email)
    {
        User? user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"User with email '{email}' not found.");
        }

        return Ok(user.Adapt<UserDto>());
    }
}
