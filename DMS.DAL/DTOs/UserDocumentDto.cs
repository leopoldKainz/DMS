namespace DMS.DAL.DTOs;

public record UserDocumentDto(
    int UserId,
    string? Username,
    int DocumentId,
    string DocumentName,
    string Permission,
    DateTime CreatedAt
);