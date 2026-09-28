namespace DMS.DAL.DTOs;

public record CreateUserDto(
    string Username,
    string Email,
    string Password
);