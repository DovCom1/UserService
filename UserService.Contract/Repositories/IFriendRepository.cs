using UserService.Model.Entities;

namespace UserService.Contract.Repositories;

public interface IFriendRepository
{
    public Task<FriendUser> AddAsync(FriendUser entity, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(FriendUser entity, CancellationToken cancellationToken = default);
    public Task<bool> DeleteAsync(FriendUser entity, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<User> friends, int total)> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default); 

}