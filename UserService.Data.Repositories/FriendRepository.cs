using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;
using UserService.Model.Enums;
using UserService.Model.Exceptions;

namespace UserService.Data.Repositories;

public class FriendRepository(ILogger<FriendRepository> logger, DataBaseContext context) : IFriendRepository
{
    private readonly ILogger<FriendRepository> _logger = logger;
    private readonly DataBaseContext _context = context;
    public async Task<FriendUser> AddAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _context.Friends.AddAsync(entity, cancellationToken);
        await TrySaveChangeAsync("Add", entity, cancellationToken, "Ошибка базы данных при отправке заявки в друзья.");
        return entity;
    }

    public async Task<FriendUser?> UpdateAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var friendUser = await _context.Friends.FindAsync([entity.UserId, entity.FriendId], cancellationToken);
        if (friendUser == null) return null;
        friendUser.Status = entity.Status;
        await TrySaveChangeAsync("Update", entity, cancellationToken, "Ошибка базы данных при обновлении статуса заявки в друзья.");
        return friendUser;
    }

    public async Task<bool> ExistsAcceptedFriendAsync(Guid userId, Guid friendId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var exists = await _context.Friends.AsNoTracking()
                .AnyAsync(f => (f.UserId == friendId && f.FriendId == userId 
                                || f.UserId == userId && f.FriendId == friendId) && f.Status == FriendStatus.Friend, cancellationToken);
        return exists;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid friendId, CancellationToken cancellationToken = default)
    {
        return await _context.Friends.AsNoTracking().AnyAsync(f =>
            (f.UserId == userId && f.FriendId == friendId)
            || (f.UserId == friendId && f.FriendId == userId), cancellationToken);
    }
    
    public async Task<bool> DeleteAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var friendUser = await _context.Friends
            .FirstOrDefaultAsync(f => (f.UserId == entity.FriendId && f.FriendId == entity.UserId 
                            || f.UserId == entity.UserId && f.FriendId == entity.FriendId), cancellationToken);

        if (friendUser == null) return false;
        _context.Friends.Remove(friendUser);
        await TrySaveChangeAsync("Delete", entity, cancellationToken, "Ошибка базы данных при удалении из списка друзей.");
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
        return await TryToListAndCountTotal(
            query,
            ct => _context.Friends.CountAsync(f => (f.UserId == userId || f.FriendId == userId) && f.Status == FriendStatus.Friend, ct),
            "GetFriendsAsync",
            cancellationToken);
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
        return await TryToListAndCountTotal(
            query,
            ct => _context.Friends.CountAsync(f => f.FriendId == userId && f.Status == FriendStatus.ApplicationSent,ct),
            "GetIncomingRequestsAsync",
            cancellationToken);
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
        return await TryToListAndCountTotal(
            query,
            ct => _context.Friends.CountAsync(f => f.UserId == userId && f.Status == FriendStatus.ApplicationSent, ct),
            "GetOutcomingRequestsAsync",
            cancellationToken);
    }
    
    // Обёртка try-catch над SaveChangesAsync
    private async Task TrySaveChangeAsync(string methodName, FriendUser entity, CancellationToken cancellationToken, string error)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Friend relationship between {entity.UserId} and FriendId {entity.FriendId} successfully {methodName}");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"{methodName}Async: Database error while processing friend relationship between {entity.UserId} and FriendId {entity.FriendId}");
            throw new UserServiceException(error, 500);
        }
    }
    
    // Обёртка try-catch над query.ToListAsync() и CountAsync
    private async Task<(IEnumerable<User> friends, int total)> TryToListAndCountTotal(IQueryable<User> query,
        Func<CancellationToken, Task<int>> countFunc,string methodName, CancellationToken cancellationToken)
    {
        try
        {
            var friends = await query.ToListAsync(cancellationToken);
            var total = await countFunc(cancellationToken);
            return (friends, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{methodName}: Database error while retrieving data");
            throw new UserServiceException($"Ошибка базы данных при получении списка.", 500);
        }
    }
}