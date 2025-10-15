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
        await CheckUserExists(entity.UserId, "AddAsync: userId", cancellationToken);
        await CheckUserExists(entity.FriendId, "AddAsync: friendId", cancellationToken);
        if (await _context.Friends.AsNoTracking().AnyAsync(f => (f.UserId == entity.UserId && f.FriendId == entity.FriendId)
                || (f.UserId == entity.FriendId && f.FriendId == entity.UserId), cancellationToken))
        {
            _logger.LogWarning($"AddAsync: Friend relationship between {entity.UserId} and {entity.FriendId} already exists");
            throw new UserServiceException("Пользователь уже находится в друзьях или заявка уже отправлена", 409);
        }
        await TrySaveChangeAsync("Add", entity, cancellationToken, "Ошибка базы данных при отправке заявки в друзья.");
        return entity;
    }

    public async Task<FriendUser> UpdateAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var friendUser = await _context.Friends.FindAsync([entity.UserId, entity.FriendId], cancellationToken);
        if (friendUser == null)
        {
            _logger.LogWarning($"UpdateAsync: Friend relationship with UserId {entity.UserId} and FriendId {entity.FriendId} not found");
            throw new UserServiceException("Этой заявки в друзья не существует", 404);
        }
        friendUser.Status = entity.Status;
        await TrySaveChangeAsync("Update", entity, cancellationToken, "Ошибка базы данных при обновлении статуса заявки в друзья.");
        return friendUser;
    }

    public async Task DeleteAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var friendUser = await _context.Friends
            .FirstOrDefaultAsync(f => (f.UserId == entity.FriendId && f.FriendId == entity.UserId 
                            || f.UserId == entity.UserId && f.FriendId == entity.FriendId) && f.Status == FriendStatus.Friend, cancellationToken);

        if (friendUser == null)
        {
            _logger.LogWarning($"DeleteAsync: Friend relationship with UserId {entity.UserId} and FriendId {entity.FriendId} not found");
            throw new UserServiceException("Пользователь не находится в списке ваших друзей", 404);
        }
        _context.Friends.Remove(friendUser);
        await TrySaveChangeAsync("Delete", entity, cancellationToken, "Ошибка базы данных при удалении друга.");
    }
    
    public async Task<bool> ExistsAsync(FriendUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var exists = await _context.Friends.AsNoTracking()
                .AnyAsync(f => (f.UserId == entity.FriendId && f.FriendId == entity.UserId 
                                || f.UserId == entity.UserId && f.FriendId == entity.FriendId) && f.Status == FriendStatus.Friend, cancellationToken);
        return exists;
    }

    public async Task<(IEnumerable<User> friends, int total)> GetFriendsAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetFriendsAsync", cancellationToken);
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
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetIncomingRequestsAsync", cancellationToken);
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
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetOutcomingRequestsAsync", cancellationToken);
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
    
    private async Task CheckUserExists(Guid userId, string methodName, CancellationToken cancellationToken)
    {
        if (!await _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId, cancellationToken))
        {
            _logger.LogWarning($"{methodName}: User with Id {userId} not found");
            throw new UserServiceException("Пользователь не существует.", 404);
        }
    } 
    
    private static void ValidatePagination(int offset, int limit)
    {
        if (offset < 0) throw new UserServiceException($"Offset не может быть отрицательным.", 400);
        if (limit <= 0) throw new UserServiceException($"Limit должен быть больше нуля.", 400);
    }
}