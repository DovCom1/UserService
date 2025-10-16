using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.FriendUser;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Exceptions;
using UserService.Model.Utilities;

namespace UserService.Service;

public class FriendManager(IFriendRepository friendRepository, IUserRepository userRepository ,IMapper mapper, ILogger<FriendManager> logger) : IFriendManager
{
    public async Task<FriendUserDTO> SendRequestAsync(CreateFriendUserDTO friendUserDto, CancellationToken ct)
    {
        await CheckUserExists(friendUserDto.UserId, "SendRequestAsync", ct);
        await CheckUserExists(friendUserDto.FriendId, "SendRequestAsync", ct);
        if (friendUserDto.UserId == friendUserDto.FriendId)
        {
            logger.LogWarning($"AddAsync: UserId {friendUserDto.UserId} cannot add self as friend");
            throw new UserServiceException("Нельзя добавить себя в друзья.", 400);
        }
        if (await friendRepository.ExistsAsync(friendUserDto.UserId, friendUserDto.FriendId, ct))
        {
            logger.LogWarning($"AddAsync: Friend relationship between {friendUserDto.UserId} and {friendUserDto.FriendId} already exists");
            throw new UserServiceException("Пользователь уже находится в списке друзей или заявка уже отправлена", 409);
        }
        var friend = await friendRepository.AddAsync(mapper.Map<FriendUser>(friendUserDto), ct);
        logger.LogInformation($"User with Id {friendUserDto.UserId} successfully sent friend request to User with Id {friendUserDto.FriendId}");
        return mapper.Map<FriendUserDTO>(friend);
    }

    public async Task<FriendUserDTO> AcceptFriendRequestAsync(UpdateFriendUserDTO friendUserDto, CancellationToken ct)
    {
        await CheckUserExists(friendUserDto.UserId, "AcceptFriendRequestAsync", ct);
        await CheckUserExists(friendUserDto.FriendId, "AcceptFriendRequestAsync", ct);
        friendUserDto = new UpdateFriendUserDTO(UserId: friendUserDto.FriendId, FriendId: friendUserDto.UserId, Status: friendUserDto.Status);
        var friendUser = await friendRepository.UpdateAsync(mapper.Map<FriendUser>(friendUserDto), ct);
        if (friendUser == null)
        {
            logger.LogWarning($"UpdateAsync: Friend relationship with UserId {friendUserDto.UserId} and FriendId {friendUserDto.FriendId} not found");
            throw new UserServiceException("Этой заявки в друзья не существует", 404);
        }
        logger.LogInformation($"User with Id {friendUserDto.FriendId} successfully accept friend request from User with Id {friendUserDto.UserId}");
        return mapper.Map<FriendUserDTO>(friendUser);
    }

    public async Task DeleteAsync(DeleteFriendUserDTO friendUserDto, CancellationToken ct)
    {
        await CheckUserExists(friendUserDto.UserId, "DeleteAsync", ct);
        await CheckUserExists(friendUserDto.FriendId, "DeleteAsync", ct);
        if (!await friendRepository.DeleteAsync(mapper.Map<FriendUser>(friendUserDto), ct))
        {
            logger.LogWarning($"DeleteAsync: Friend relationship with UserId {friendUserDto.UserId} and FriendId {friendUserDto.FriendId} not found");
            throw new UserServiceException("Пользователь не находится в списке ваших друзей", 404);
        }
        logger.LogInformation($"User with Id {friendUserDto.UserId} successfully deleted User with Id {friendUserDto.FriendId} from friends");
    }

    public async Task<bool> CheckFriendExists(Guid userId, Guid friendId, CancellationToken ct)
    {
        await CheckUserExists(userId, "CheckFriendExists", ct);
        await CheckUserExists(friendId, "CheckFriendExists", ct);
        return await friendRepository.ExistsAcceptedFriendAsync(userId, friendId, ct);
    }

    public async Task<PagedFriendResponseDTO> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetFriendsAsync", ct);
        var (friends, total) = await friendRepository.GetFriendsAsync(userId, offset, limit, ct);
        var data = friends.Select(friend => new ShortUserDTO(
            Id: friend.Id,
            Uid: friend.Uid,
            Nickname: friend.Nickname,
            AvatarUrl: friend.AvatarUrl,
            Status: friend.Status.GetDescription()
        )).ToList();
        return new PagedFriendResponseDTO(data, offset, limit, total);
    }
    
    public async Task<PagedFriendResponseDTO> GetIncomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetIncomingRequestsAsync", ct);
        var (friends, total) = await friendRepository.GetIncomingRequestsAsync(userId, offset, limit, ct);
        var data = friends.Select(friend => new ShortUserDTO(
            Id: friend.Id,
            Uid: friend.Uid,
            Nickname: friend.Nickname,
            AvatarUrl: friend.AvatarUrl,
            Status: friend.Status.GetDescription()
        )).ToList();
        return new PagedFriendResponseDTO(data, offset, limit, total);
    }
    
    public async Task<PagedFriendResponseDTO> GetOutcomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetOutcomingRequestsAsync", ct);
        var (friends, total) = await friendRepository.GetOutcomingRequestsAsync(userId, offset, limit, ct);
        var data = friends.Select(friend => new ShortUserDTO(
            Id: friend.Id,
            Uid: friend.Uid,
            Nickname: friend.Nickname,
            AvatarUrl: friend.AvatarUrl,
            Status: friend.Status.GetDescription()
        )).ToList();
        return new PagedFriendResponseDTO(data, offset, limit, total);
    }

    private async Task CheckUserExists(Guid userId, string methodName, CancellationToken ct)
    {
        if (!await userRepository.ExistsAsync(userId, ct))
        {
            logger.LogWarning($"{methodName}: User with Id {userId} not found");
            throw new UserServiceException("Пользователь не существует.", 404);
        }
    }
    
    private static void ValidatePagination(int offset, int limit)
    {
        if (offset < 0) throw new UserServiceException($"Offset не может быть отрицательным.", 400);
        if (limit <= 0) throw new UserServiceException($"Limit должен быть больше нуля.", 400);
        if (limit > 20) throw new UserServiceException($"Limit не может превышать 20.", 400);
    }
}