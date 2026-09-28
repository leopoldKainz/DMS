namespace DMS.DAL.DTOs;

public record CreateShareRequestDto(
    int OwnerId,
    int RecieverId,
    int DocumentId
);