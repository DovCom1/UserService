using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;
using UserService.Model.Enums;

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

    public async Task<FriendUser?> UpdateAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var friendUser = await _context.Friends.FindAsync([entity.UserId, entity.FriendId], cancellationToken);
        if (friendUser == null)
        {
            _logger.LogWarning($"UpdateAsync: Friend relationship with UserId {entity.UserId} and EnemyId {entity.FriendId} not found");
            return null;
        }
        friendUser.Status = entity.Status;
        await _context.SaveChangesAsync(cancellationToken);
        return friendUser;
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
            _logger.LogWarning($"RemoveAsync: Friend relationship with UserId {entity.UserId} and EnemyId {entity.FriendId} not found");
            return false;
        }
        _context.Friends.Remove(friendUser);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IEnumerable<User> friends, int total)> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = _context.Friends
            .AsNoTracking()
            .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Friend)
            .Include(f => f.Friend)
            .Include(f => f.User)
            .Select(f => f.FriendId == userId ? f.User : f.Friend)
            .Skip(offset)
            .Take(limit);
        var friends = await query.ToListAsync(cancellationToken);
        var total = await _context.Friends.CountAsync(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Friend,cancellationToken);
        return (friends, total);
    }

    public async Task<(IEnumerable<User> friends, int total)> GetIncomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = _context.Friends
            .AsNoTracking()
            .Where(f => f.FriendId == userId && f.Status == FriendStatus.ApplicationSent)
            .Include(f => f.User)
            .Select(f => f.User)
            .Skip(offset)
            .Take(limit);
        var friends = await query.ToListAsync(cancellationToken);
        var total = await _context.Friends.CountAsync(f => f.FriendId == userId && f.Status == FriendStatus.ApplicationSent,cancellationToken);
        return (friends, total);
    }

    public async Task<(IEnumerable<User> friends, int total)> GetOutcomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = _context.Friends
            .AsNoTracking()
            .Where(f => f.UserId == userId && f.Status == FriendStatus.ApplicationSent)
            .Include(f => f.Friend)
            .Select(f => f.Friend)
            .Skip(offset)
            .Take(limit);
        var friends = await query.ToListAsync(cancellationToken);
        var total = await _context.Friends.CountAsync(f => f.UserId == userId && f.Status == FriendStatus.ApplicationSent,cancellationToken);
        return (friends, total);
    }
}