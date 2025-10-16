using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.FriendUser;
using UserService.Model.Entities;
using UserService.Model.Exceptions;

namespace UserService.Service;

public class FriendManager(IFriendRepository friendRepository, IUserRepository userRepository ,IMapper mapper, ILogger<FriendManager> logger) : IFriendManager
{
    private readonly IFriendRepository _friendRepository = friendRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<FriendManager> _logger = logger;
    public async Task<FriendUserDTO> SendRequestAsync(CreateFriendUserDTO friendUserDto, CancellationToken cancellationToken)
    {
        await CheckUserExists(friendUserDto.UserId, "SendRequestAsync", cancellationToken);
        await CheckUserExists(friendUserDto.FriendId, "SendRequestAsync", cancellationToken);
        if (friendUserDto.UserId == friendUserDto.FriendId)
        {
            _logger.LogWarning($"AddAsync: UserId {friendUserDto.UserId} cannot add self as friend");
            throw new UserServiceException("Нельзя добавить себя в друзья.", 400);
        }
        if (await _friendRepository.ExistsAsync(friendUserDto.UserId, friendUserDto.FriendId, cancellationToken))
        {
            _logger.LogWarning($"AddAsync: Friend relationship between {friendUserDto.UserId} and {friendUserDto.FriendId} already exists");
            throw new UserServiceException("Пользователь уже находится в списке друзей или заявка уже отправлена", 409);
        }
        var friend = await _friendRepository.AddAsync(_mapper.Map<FriendUser>(friendUserDto), cancellationToken);
        _logger.LogInformation($"User with Id {friendUserDto.UserId} successfully sent friend request to User with Id {friendUserDto.FriendId}");
        return _mapper.Map<FriendUserDTO>(friend);
    }

    public async Task<FriendUserDTO> AcceptFriendRequestAsync(UpdateFriendUserDTO friendUserDto,
        CancellationToken cancellationToken)
    {
        await CheckUserExists(friendUserDto.UserId, "AcceptFriendRequestAsync", cancellationToken);
        await CheckUserExists(friendUserDto.FriendId, "AcceptFriendRequestAsync", cancellationToken);
        friendUserDto = new UpdateFriendUserDTO(UserId: friendUserDto.FriendId, FriendId: friendUserDto.UserId, Status: friendUserDto.Status);
        var friendUser = await _friendRepository.UpdateAsync(_mapper.Map<FriendUser>(friendUserDto), cancellationToken);
        if (friendUser == null)
        {
            _logger.LogWarning($"UpdateAsync: Friend relationship with UserId {friendUserDto.UserId} and FriendId {friendUserDto.FriendId} not found");
            throw new UserServiceException("Этой заявки в друзья не существует", 404);
        }
        _logger.LogInformation($"User with Id {friendUserDto.FriendId} successfully accept friend request from User with Id {friendUserDto.UserId}");
        return _mapper.Map<FriendUserDTO>(friendUser);
    }

    public async Task DeleteAsync(DeleteFriendUserDTO friendUserDto, CancellationToken cancellationToken)
    {
        await CheckUserExists(friendUserDto.UserId, "DeleteAsync", cancellationToken);
        await CheckUserExists(friendUserDto.FriendId, "DeleteAsync", cancellationToken);
        var friendUser = _mapper.Map<FriendUser>(friendUserDto);
        if (!await _friendRepository.DeleteAsync(friendUser, cancellationToken))
        {
            _logger.LogWarning($"DeleteAsync: Friend relationship with UserId {friendUserDto.UserId} and FriendId {friendUserDto.FriendId} not found");
            throw new UserServiceException("Пользователь не находится в списке ваших друзей", 404);
        }
        _logger.LogInformation($"User with Id {friendUserDto.UserId} successfully deleted User with Id {friendUserDto.FriendId} from friends");
    }

    public async Task<bool> CheckFriendExists(Guid userId, Guid friendId, CancellationToken cancellationToken)
    {
        await CheckUserExists(userId, "CheckFriendExists", cancellationToken);
        await CheckUserExists(friendId, "CheckFriendExists", cancellationToken);
        return await _friendRepository.ExistsAcceptedFriendAsync(userId, friendId, cancellationToken);
    }

    public async Task<PagedFriendResponseDTO> GetFriendsAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetFriendsAsync", cancellationToken);
        var (friends, total) = await _friendRepository.GetFriendsAsync(userId, offset, limit, cancellationToken);
        var page = _mapper.Map<PagedFriendResponseDTO>(friends);
        return page with { Total = total, Offset = offset, Limit = limit };
    }
    
    public async Task<PagedFriendResponseDTO> GetIncomingRequestsAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetIncomingRequestsAsync", cancellationToken);
        var (friends, total) = await _friendRepository.GetIncomingRequestsAsync(userId, offset, limit, cancellationToken);
        var page = _mapper.Map<PagedFriendResponseDTO>(friends);
        return page with { Total = total, Offset = offset, Limit = limit };
    }
    
    public async Task<PagedFriendResponseDTO> GetOutcomingRequestsAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetOutcomingRequestsAsync", cancellationToken);
        var (friends, total) = await _friendRepository.GetOutcomingRequestsAsync(userId, offset, limit, cancellationToken);
        var page = _mapper.Map<PagedFriendResponseDTO>(friends);
        return page with { Total = total, Offset = offset, Limit = limit };
    }

    private async Task CheckUserExists(Guid userId, string methodName, CancellationToken cancellationToken)
    {
        if (!await _userRepository.ExistsAsync(userId, cancellationToken))
        {
            _logger.LogWarning($"{methodName}: User with Id {userId} not found");
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