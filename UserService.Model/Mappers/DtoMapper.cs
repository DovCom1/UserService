using AutoMapper;
using UserService.Model.DTO.EnemyUser;
using UserService.Model.DTO.FriendUser;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Enums;
using UserService.Model.Utilities;

namespace UserService.Model.Mappers;

public class DtoMapper : Profile
{
    public DtoMapper()
    {
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.GetDescription()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.GetDescription()));

        CreateMap<User, ShortUserDTO>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.GetDescription()));
        
        CreateMap<CreateUserDTO, User>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                Enum.GetValues<Gender>().First(g => g.GetDescription() == src.Gender)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => UserStatus.Online))
            .ForMember(dest => dest.AccountCreationTime, opt => opt.MapFrom(_ => DateTime.UtcNow));
        
        CreateMap<UpdateFriendUserDTO, FriendUser>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                Enum.GetValues<FriendStatus>().First(s => s.GetDescription() == src.Status)));

        CreateMap<CreateFriendUserDTO, FriendUser>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => FriendStatus.ApplicationSent));
        CreateMap<CreateEnemyUserDTO, EnemyUser>();
        CreateMap<DeleteFriendUserDTO, FriendUser>();
        CreateMap<EnemyUser, EnemyUserDTO>();
        CreateMap<EnemyUserDTO, EnemyUser>();
        
        CreateMap<FriendUser, FriendUserDTO>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.GetDescription()));
    }
}