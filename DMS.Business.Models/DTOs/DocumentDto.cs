namespace DMS.DAL.DTOs;

public record DocumentDto(
    Guid Id,
    string Name,
    string Type,
    long Size,
    DateTime CreatedAt
);
