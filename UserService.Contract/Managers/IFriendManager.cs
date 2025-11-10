using UserService.Model.DTO.FriendUser;

namespace UserService.Contract.Managers;

public interface IFriendManager
{
    public Task<FriendUserDTO> SendRequestAsync(CreateFriendUserDTO friendUserDto, CancellationToken ct);
    public Task<FriendUserDTO> AcceptFriendRequestAsync(UpdateFriendUserDTO friendUserDto, CancellationToken ct);
    public Task RejectFriendRequestAsync(DeleteFriendUserDTO friendUserDto, CancellationToken ct);
    public Task DeleteAsync(DeleteFriendUserDTO friendUserDto, CancellationToken ct);
    public Task<bool> IsAcceptedFriendAsync(Guid userId, Guid friendId, CancellationToken ct);
    public Task<bool> IsPendingOrAcceptedFriendAsync(Guid userId, Guid friendId, CancellationToken ct);
    public Task<PagedFriendResponseDTO> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken ct);
    public Task<PagedFriendResponseDTO> GetIncomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct);
    public Task<PagedFriendResponseDTO> GetOutcomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct);
}