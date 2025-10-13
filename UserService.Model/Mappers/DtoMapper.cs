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
            .ForMember(dest => dest.AccountCreationTime, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<UpdateUserDTO, User>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.Gender) ? (Gender?)null : Enum.GetValues<Gender>().First(g => g.GetDescription() == src.Gender)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.Status) ? (UserStatus?)null : Enum.GetValues<UserStatus>().First(s => s.GetDescription() == src.Status)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<IEnumerable<FriendUser>, PagedFriendResponseDTO>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => 
                src.Select(f =>  new ShortUserDTO(f.FriendId, f.Friend.Uid, f.Friend.Nickname, f.Friend.AvatarUrl, f.Friend.Status.GetDescription()))))
            .ForMember(dest => dest.Offset, opt => opt.Ignore())
            .ForMember(dest => dest.Limit, opt => opt.Ignore())
            .ForMember(dest => dest.Total, opt => opt.Ignore());
        
        CreateMap<IEnumerable<EnemyUser>, PagedEnemyResponseDTO>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                src.Select(f => new ShortUserDTO(f.EnemyId, f.Enemy.Uid, f.Enemy.Nickname, f.Enemy.AvatarUrl, f.Enemy.Status.GetDescription()))))
            .ForMember(dest => dest.Offset, opt => opt.Ignore())
            .ForMember(dest => dest.Limit, opt => opt.Ignore())
            .ForMember(dest => dest.Total, opt => opt.Ignore());

        CreateMap<UpdateFriendUserDTO, FriendUser>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                Enum.GetValues<FriendStatus>().First(s => s.GetDescription() == src.Status)));

        CreateMap<CreateFriendUserDTO, FriendUser>();
        CreateMap<CreateEnemyUserDTO, EnemyUser>();
    }
}