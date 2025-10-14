using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;

namespace UserService.Data.Repositories;

public class FriendRepository(ILogger<FriendRepository> logger, DataBaseContext context) : IFriendRepository
{
    private readonly ILogger<FriendRepository> _logger = logger;
    private readonly DataBaseContext _context = context;
    public async Task<FriendUser> AddAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _context.Friends.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> ExistsAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var friendUser =
            await _context.Friends.FindAsync([entity.UserId, entity.FriendId], cancellationToken);
        return friendUser != null;
    }

    public async Task<bool> DeleteAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var friendUser =
            await _context.Friends.FindAsync([entity.UserId, entity.FriendId], cancellationToken);
        if (friendUser == null)
        {
            _logger.LogWarning($"RemoveAsync: Enemy relationship with UserId {entity.UserId} and EnemyId {entity.FriendId} not found");
            return false;
        }
        _context.Friends.Remove(friendUser);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IEnumerable<User> friends, int total)> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}