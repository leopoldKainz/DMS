namespace DMS.DAL.DTOs;

public record LoginDto(
    string UsernameOrEmail,
    string Password
);
