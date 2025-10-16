using UserService.Model.DTO.FriendUser;

namespace UserService.Contract.Managers;

public interface IFriendManager
{
    public Task<FriendUserDTO> SendRequestAsync(CreateFriendUserDTO friendUserDto, CancellationToken cancellationToken);
    public Task<FriendUserDTO> AcceptFriendRequestAsync(UpdateFriendUserDTO friendUserDto,
        CancellationToken cancellationToken);
    public Task DeleteAsync(DeleteFriendUserDTO friendUserDto, CancellationToken cancellationToken);
    public Task<bool> CheckFriendExists(Guid userId, Guid friendId, CancellationToken cancellationToken);
    public Task<PagedFriendResponseDTO> GetFriendsAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken);
    public Task<PagedFriendResponseDTO> GetIncomingRequestsAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken);
    public Task<PagedFriendResponseDTO> GetOutcomingRequestsAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken);
}