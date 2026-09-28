namespace DMS.DAL.DTOs;

public record UserDto(
    int Id,
    string? Username,
    string? Email,
    DateTime CreatedAt
);