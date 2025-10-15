using UserService.Model.Entities;

namespace UserService.Contract.Repositories;

public interface IFriendRepository
{
    public Task<FriendUser> AddAsync(FriendUser entity, CancellationToken cancellationToken = default);
    public Task<FriendUser> UpdateAsync(FriendUser entity, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(FriendUser entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(FriendUser entity, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<User> friends, int total)> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default); 
    public Task<(IEnumerable<User> friends, int total)> GetIncomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default); 
    public Task<(IEnumerable<User> friends, int total)> GetOutcomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default); 
}