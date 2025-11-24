using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Contract.Services;
using UserService.Model.DTO.EnemyUser;
using UserService.Model.DTO.FriendUser;
using UserService.Model.DTO.Notify;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Exceptions;
using UserService.Model.Utilities;
using UserService.Service.Extensions;

namespace UserService.Service.Managers;

public class FriendManager(INotifierService notifierService, IFriendRepository friendRepository, IUserManager userManager, IEnemyRepository enemyRepository, ILogger<FriendManager> logger) : IFriendManager
{
    public async Task<FriendUserDTO> SendRequestAsync(CreateFriendUserDTO friendUserDto, CancellationToken ct)
    {
        await userManager.ExistsAsync(friendUserDto.UserId, ct);
        await userManager.ExistsAsync(friendUserDto.FriendId, ct);
        if (friendUserDto.UserId == friendUserDto.FriendId)
        {
            logger.LogWarning($"FriendManager(Add): UserId {friendUserDto.UserId} cannot add self as friend");
            throw new UserServiceException("Нельзя добавить себя в друзья.", 400);
        }
        if (await enemyRepository.IsEnemy(friendUserDto.UserId, friendUserDto.FriendId, ct))
        {
            var dto = new EnemyUserDTO(friendUserDto.UserId, friendUserDto.FriendId);
            await enemyRepository.DeleteAsync(dto.ToEnemyUser(), ct);
        }
        if (await friendRepository.IsPendingOrAccepted(friendUserDto.UserId, friendUserDto.FriendId, ct))
        {
            logger.LogWarning($"FriendManager(Add): Friend relationship between {friendUserDto.UserId} and {friendUserDto.FriendId} already exists");
            throw new UserServiceException("Пользователь уже находится в списке друзей или заявка уже отправлена", 409);
        }
        if (await enemyRepository.IsEnemy(friendUserDto.FriendId, friendUserDto.UserId, ct))
        {
            logger.LogWarning(
                $"FriendManager(Add): Cannot send friend request from user {friendUserDto.UserId} to {friendUserDto.FriendId} — target user has added sender to enemies list");
            throw new UserServiceException("Невозможно отправить заявку: вы находитель в списке врагов пользователя.", 403);
        }
        var friend = await friendRepository.AddAsync(friendUserDto.ToFriendUser(), ct);
        logger.LogInformation($"User with Id {friendUserDto.UserId} successfully sent friend request to User with Id {friendUserDto.FriendId}");
        var notifyBody = new FriendRequestDTO(friend.UserId, friend.FriendId, friend.User.Nickname, friend.Friend.Nickname, DateTime.Now);
        await notifierService.NotifySendFriendRequestAsync(notifyBody, ct);
        return friend.ToFriendUserDto();
    }

    public async Task<FriendUserDTO> AcceptFriendRequestAsync(UpdateFriendUserDTO friendUserDto, CancellationToken ct)
    {
        await userManager.ExistsAsync(friendUserDto.UserId, ct);
        await userManager.ExistsAsync(friendUserDto.FriendId, ct);
        friendUserDto = new UpdateFriendUserDTO(UserId: friendUserDto.FriendId, FriendId: friendUserDto.UserId, Status: friendUserDto.Status);
        var friendUser = await friendRepository.UpdateAsync(friendUserDto.ToFriendUser(), ct);
        if (friendUser == null)
        {
            logger.LogWarning($"FriendManager(Update): Friend relationship with UserId {friendUserDto.UserId} and FriendId {friendUserDto.FriendId} not found");
            throw new UserServiceException("Этой заявки в друзья не существует", 404);
        }
        logger.LogInformation($"User with Id {friendUserDto.FriendId} successfully accepted friend request from User with Id {friendUserDto.UserId}");
        return friendUser.ToFriendUserDto();
    }

    public async Task RejectFriendRequestAsync(DeleteFriendUserDTO friendUserDto, CancellationToken ct)
    {
        await userManager.ExistsAsync(friendUserDto.UserId, ct);
        await userManager.ExistsAsync(friendUserDto.FriendId, ct);
        if (!await friendRepository.DeleteAsync(friendUserDto.ToFriendUser(), ct))
        {
            logger.LogWarning($"FriendManager(RejectFriendRequest): Friend request from User with Id {friendUserDto.FriendId} to User with Id {friendUserDto.UserId} not found");
            throw new UserServiceException("Такой заявки не существует", 404);
        }
        logger.LogInformation($"User with Id {friendUserDto.FriendId} successfully rejected friend request from User with Id {friendUserDto.UserId}");
    }

    public async Task DeleteAsync(DeleteFriendUserDTO friendUserDto, CancellationToken ct)
    {
        await userManager.ExistsAsync(friendUserDto.UserId, ct);
        await userManager.ExistsAsync(friendUserDto.FriendId, ct);
        if (!await friendRepository.DeleteAsync(friendUserDto.ToFriendUser(), ct))
        {
            logger.LogWarning($"FriendManager(Delete): Friend relationship with UserId {friendUserDto.UserId} and FriendId {friendUserDto.FriendId} not found");
            throw new UserServiceException("Пользователь не находится в списке ваших друзей", 404);
        }
        logger.LogInformation($"User with Id {friendUserDto.UserId} successfully deleted User with Id {friendUserDto.FriendId} from friends");
    }

    public async Task<bool> IsAcceptedFriendAsync(Guid userId, Guid friendId, CancellationToken ct)
    {
        await userManager.ExistsAsync(userId, ct);
        await userManager.ExistsAsync(friendId, ct);
        return await friendRepository.IsAcceptedAsync(userId, friendId, ct);
    }
    
    public async Task<bool> IsPendingOrAcceptedFriendAsync(Guid userId, Guid friendId, CancellationToken ct)
    {
        await userManager.ExistsAsync(userId, ct);
        await userManager.ExistsAsync(friendId, ct);
        return await friendRepository.IsPendingOrAccepted(userId, friendId, ct);
    }

    public async Task<PagedFriendResponseDTO> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        await userManager.ExistsAsync(userId, ct);
        var (friends, total) = await friendRepository.GetFriendsAsync(userId, offset, limit, ct);
        var data = friends.Select(friend => friend.ToShortUserDto()).ToList();
        return new PagedFriendResponseDTO(data, offset, limit, total);
    }
    
    public async Task<PagedFriendResponseDTO> GetIncomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        await userManager.ExistsAsync(userId, ct);
        var (friends, total) = await friendRepository.GetIncomingRequestsAsync(userId, offset, limit, ct);
        var data = friends.Select(friend => friend.ToShortUserDto()).ToList();
        return new PagedFriendResponseDTO(data, offset, limit, total);
    }
    
    public async Task<PagedFriendResponseDTO> GetOutcomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        await userManager.ExistsAsync(userId, ct);
        var (friends, total) = await friendRepository.GetOutcomingRequestsAsync(userId, offset, limit, ct);
        var data = friends.Select(friend => friend.ToShortUserDto()).ToList();
        return new PagedFriendResponseDTO(data, offset, limit, total);
    }
    
    private static void ValidatePagination(int offset, int limit)
    {
        if (offset < 0) throw new UserServiceException($"Offset не может быть отрицательным.", 400);
        if (limit <= 0) throw new UserServiceException($"Limit должен быть больше нуля.", 400);
        if (limit > 20) throw new UserServiceException($"Limit не может превышать 20.", 400);
    }
}