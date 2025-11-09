using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;
using UserService.Model.Enums;
using UserService.Model.Exceptions;

namespace UserService.Data.Repositories;

public class FriendRepository(DataBaseContext context) : IFriendRepository
{
    public async Task<FriendUser> AddAsync(FriendUser entity, CancellationToken ct = default)
    {
        await context.Friends.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<FriendUser?> UpdateAsync(FriendUser entity, CancellationToken ct = default)
    {
        var friendUser = await context.Friends.FindAsync([entity.UserId, entity.FriendId], ct);
        if (friendUser == null) return null;
        friendUser.Status = entity.Status;
        await context.SaveChangesAsync(ct);
        return friendUser;
    }

    public async Task<bool> ExistsAcceptedFriendAsync(Guid userId, Guid friendId, CancellationToken ct = default)
    {
        var exists = await context.Friends.AsNoTracking()
                .AnyAsync(f => (f.UserId == friendId && f.FriendId == userId 
                                || f.UserId == userId && f.FriendId == friendId) && f.Status == FriendStatus.Friend, ct);
        return exists;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid friendId, CancellationToken ct = default)
    {
        return await context.Friends.AsNoTracking().AnyAsync(f =>
            (f.UserId == userId && f.FriendId == friendId)
            || (f.UserId == friendId && f.FriendId == userId), ct);
    }
    
    public async Task<bool> DeleteAsync(FriendUser entity, CancellationToken ct = default)
    {
        var friendUser = await context.Friends
            .FirstOrDefaultAsync(f => (f.UserId == entity.FriendId && f.FriendId == entity.UserId 
                            || f.UserId == entity.UserId && f.FriendId == entity.FriendId), ct);

        if (friendUser == null) return false;
        context.Friends.Remove(friendUser);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(IEnumerable<User> friends, int total)> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken ct = default)
    {
        var query = context.Friends
            .AsNoTracking()
            .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Friend)
            .Include(f => f.Friend)
            .Include(f => f.User)
            .OrderBy(f => f.UserId)
            .ThenBy(f => f.FriendId)
            .Select(f => f.FriendId == userId ? f.User : f.Friend)
            .Skip(offset)
            .Take(limit);
        var friends = await query.ToListAsync(ct);
        var total = await context.Friends.CountAsync(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Friend, ct);
        return (friends, total);
    }

    public async Task<(IEnumerable<User> friends, int total)> GetIncomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct = default)
    {
        var query = context.Friends
            .AsNoTracking()
            .Where(f => f.FriendId == userId && f.Status == FriendStatus.ApplicationSent)
            .Include(f => f.User)
            .Select(f => f.User)
            .OrderBy(u => u.Id)
            .Skip(offset)
            .Take(limit);
        var friends = await query.ToListAsync(ct);
        var total = await context.Friends.CountAsync(f => f.FriendId == userId && f.Status == FriendStatus.ApplicationSent, ct);
        return (friends, total);
    }

    public async Task<(IEnumerable<User> friends, int total)> GetOutcomingRequestsAsync(Guid userId, int offset, int limit, CancellationToken ct = default)
    {
        var query = context.Friends
            .AsNoTracking()
            .Where(f => f.UserId == userId && f.Status == FriendStatus.ApplicationSent)
            .Include(f => f.Friend)
            .Select(f => f.Friend)
            .OrderBy(u => u.Id)
            .Skip(offset)
            .Take(limit);
        var friends = await query.ToListAsync(ct);
        var total = await context.Friends.CountAsync(f => f.UserId == userId && f.Status == FriendStatus.ApplicationSent, ct);
        return (friends, total);
    }
}