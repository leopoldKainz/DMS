namespace DMS.DAL.DTOs;

public record ShareRequestDto(
    Guid Id,
    Guid OwnerId,
    Guid RecieverId,
    Guid DocumentId,
    DateTime CreatedAt
);
