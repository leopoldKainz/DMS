namespace DMS.DAL.DTOs;

public record DocumentDto(
    int Id,
    string Name,
    string Type,
    long Size,
    DateTime CreatedAt
);