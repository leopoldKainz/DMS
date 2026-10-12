namespace DMS.DAL.DTOs;

public record LoginResponseDto(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);
