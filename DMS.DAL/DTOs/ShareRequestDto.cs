namespace DMS.DAL.DTOs;

public record ShareRequestDto(
    int Id,
    UserDto Owner,
    UserDto Reciever,
    DocumentDto Document,
    DateTime CreatedAt
);