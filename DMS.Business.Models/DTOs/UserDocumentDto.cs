namespace DMS.DAL.DTOs;

public record UserDocumentDto(
    Guid UserId,
    string? Username,
    Guid DocumentId,
    string DocumentName,
    string Permission,
    DateTime CreatedAt
);
