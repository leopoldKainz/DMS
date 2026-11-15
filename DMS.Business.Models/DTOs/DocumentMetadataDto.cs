namespace DMS.DAL.DTOs;

public record DocumentMetadataDto(
    string? ContentType,
    int? PageCount
);
