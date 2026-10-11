namespace DMS.DAL.DTOs;

public record CreateShareRequestDto(
    Guid OwnerId,
    Guid RecieverId,
    Guid DocumentId
);
