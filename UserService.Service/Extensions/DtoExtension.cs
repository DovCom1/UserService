using UserService.Model.DTO.EnemyUser;
using UserService.Model.DTO.FriendUser;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Enums;
using UserService.Model.Utilities;

namespace UserService.Service.Extensions;

public static class DtoExtension
{
    public static User ToUser(this CreateUserDTO dto)
        => new User(dto.Uid, dto.Nickname, dto.Email, dto.Gender, dto.DateOfBirth);
    
    public static UserDTO ToUserDto(this User user)
        => new UserDTO(user.Id, user.Uid, user.Nickname, user.Email, user.AvatarUrl, user.Gender.GetDescription(),
            user.Status.GetDescription(), user.DateOfBirth, DateOnly.FromDateTime(user.AccountCreationTime));

    public static ShortUserDTO ToShortUserDto(this User user)
        => new ShortUserDTO(user.Id, user.Uid, user.Nickname, user.AvatarUrl, user.Status.GetDescription());

    public static void UpdateFromDto(this User user, UpdateUserDTO dto)
    {
        if (dto.Uid != null) user.Uid = dto.Uid;
        if (dto.Nickname != null) user.Nickname = dto.Nickname;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
        if (dto.Gender != null) user.Gender = dto.Gender.ParseByDescription<Gender>();
        if (dto.Status != null) user.Status = dto.Status.ParseByDescription<UserStatus>();
        if (dto.DateOfBirth != null) user.DateOfBirth = dto.DateOfBirth.Value;
    }

    public static FriendUser ToFriendUser(this CreateFriendUserDTO dto)
        => new FriendUser(dto.UserId, dto.FriendId, FriendStatus.ApplicationSent);
    
    public static FriendUserDTO ToFriendUserDto(this FriendUser fu)
        => new FriendUserDTO(fu.UserId, fu.FriendId, fu.Status.GetDescription());

    public static FriendUser ToFriendUser(this UpdateFriendUserDTO dto)
        => new FriendUser(dto.UserId, dto.FriendId, dto.Status.ParseByDescription<FriendStatus>());

    public static FriendUser ToFriendUser(this DeleteFriendUserDTO dto)
        => new FriendUser(dto.UserId, dto.FriendId);

    public static EnemyUser ToEnemyUser(this CreateEnemyUserDTO dto)
        => new EnemyUser(dto.UserId, dto.EnemyId);

    public static EnemyUserDTO ToEnemyUserDto(this EnemyUser eu)
        => new EnemyUserDTO(eu.UserId, eu.EnemyId);
    
    public static EnemyUser ToEnemyUser(this EnemyUserDTO dto)
        => new EnemyUser(dto.UserId, dto.EnemyId);
}