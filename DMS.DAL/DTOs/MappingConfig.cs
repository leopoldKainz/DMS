using DMS.DAL.Entities;
using Mapster;

namespace DMS.DAL.DTOs;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // User -> UserDto: Password und Navigationslisten ignorieren
        config.NewConfig<User, UserDto>();

        // CreateUserDto -> User
        config.NewConfig<CreateUserDto, User>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedOn);

        // Document -> DocumentDto
        config.NewConfig<Document, DocumentDto>();

        // UserDocument -> UserDocumentDto: Flattening für verknüpfte Entitäten
        config.NewConfig<UserDocument, UserDocumentDto>()
            .Map(dest => dest.Username, src => src.User != null ? src.User.Username : null)
            .Map(dest => dest.DocumentName, src => src.Document != null ? src.Document.Name : null)
            .Map(dest => dest.Permission, src => src.Permission.ToString());

        // ShareRequest -> ShareRequestDto
        config.NewConfig<ShareRequest, ShareRequestDto>();
    }
}