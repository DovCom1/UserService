using UserService.Model.Entities;

namespace UserService.Contract.Repositories;

public interface IFriendRepository
{
    public Task<FriendUser> AddAsync(FriendUser entity, CancellationToken ct = default);
    public Task<FriendUser?> UpdateAsync(FriendUser entity, CancellationToken ct = default);
    public Task<bool> ExistsAcceptedFriendAsync(Guid userId, Guid friendId, CancellationToken ct = default);
    public Task<bool> ExistsAsync(Guid userId, Guid friendId, CancellationToken ct = default);
    public Task<bool> DeleteAsync(FriendUser entity, CancellationToken ct = default);
    public Task<(IEnumerable<User> friends, int total)> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken ct = default); 
    public Task<(IEnumerable<User> friends, int total)> GetIncomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct = default); 
    public Task<(IEnumerable<User> friends, int total)> GetOutcomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct = default);
}