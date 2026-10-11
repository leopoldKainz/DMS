using DMS.DAL.Entities;
using Mapster;

namespace DMS.DAL.DTOs;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Id, src => src.EntityId);

        config.NewConfig<CreateUserDto, User>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.EntityId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedOn);

        config.NewConfig<Document, DocumentDto>()
            .Map(dest => dest.Id, src => src.EntityId);

        config.NewConfig<UserDocument, UserDocumentDto>()
            .Map(dest => dest.UserId, src => src.User != null ? src.User.EntityId : Guid.Empty)
            .Map(dest => dest.Username, src => src.User != null ? src.User.Username : null)
            .Map(dest => dest.DocumentId, src => src.Document != null ? src.Document.EntityId : Guid.Empty)
            .Map(dest => dest.DocumentName, src => src.Document != null ? src.Document.Name : null)
            .Map(dest => dest.Permission, src => src.Permission.ToString());

        config.NewConfig<ShareRequest, ShareRequestDto>()
            .Map(dest => dest.Id, src => src.EntityId);
    }
}
