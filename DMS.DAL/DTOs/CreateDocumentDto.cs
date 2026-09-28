namespace DMS.DAL.DTOs;

public record CreateDocumentDto(
    string Name,
    string Type,
    long Size
);