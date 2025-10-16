using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;
using UserService.Model.Exceptions;

namespace UserService.Data.Repositories;

public class EnemyRepository(ILogger<EnemyRepository> logger, DataBaseContext context) : IEnemyRepository
{
    private readonly DataBaseContext _context = context;
    private readonly ILogger<EnemyRepository> _logger = logger;

    public async Task<EnemyUser> AddAsync(EnemyUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _context.Enemies.AddAsync(entity, cancellationToken);
        await TrySaveChangeAsync("Add", entity, cancellationToken, "Ошибка базы данных при добавлении во враги.");
        return entity;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid enemyId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _context.Enemies.AsNoTracking()
            .AnyAsync(e => e.UserId == userId && e.EnemyId == enemyId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(EnemyUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var enemyUser =
            await _context.Enemies.FindAsync([entity.UserId, entity.EnemyId], cancellationToken);
        if (enemyUser == null) return false;
        _context.Enemies.Remove(enemyUser);
        await TrySaveChangeAsync("Delete", entity, cancellationToken, "Ошибка базы данных при удалении из врагов.");
        return true;
    }

    public async Task<(IEnumerable<User> enemies, int total)> GetEnemiesAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = _context.Enemies
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Enemy)
            .Select(e => e.Enemy)
            .Skip(offset)
            .Take(limit);
        return await TryToListAndCountTotal(
            query,
            ct =>  _context.Enemies.CountAsync(e => e.UserId == userId, ct),
            "GetEnemiesAsync",
            cancellationToken);
    }
    
    // Обёртка try-catch над SaveChangesAsync
    private async Task TrySaveChangeAsync(string methodName, EnemyUser entity, CancellationToken cancellationToken, string error)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Enemy relationship between {entity.UserId} and EnemyId {entity.EnemyId} successfully {methodName}");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"{methodName}Async: Database error while processing enemy relationship between {entity.UserId} and EnemyId {entity.EnemyId}");
            throw new UserServiceException(error, 500);
        }
    }
    
    // Обёртка try-catch над query.ToListAsync() и CountAsync
    private async Task<(IEnumerable<User> enemies, int total)> TryToListAndCountTotal(IQueryable<User> query,
        Func<CancellationToken, Task<int>> countFunc,string methodName, CancellationToken cancellationToken)
    {
        try
        {
            var enemies = await query.ToListAsync(cancellationToken);
            var total = await countFunc(cancellationToken);
            return (enemies, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{methodName}: Database error while retrieving data");
            throw new UserServiceException($"Ошибка базы данных при получении списка.", 500);
        }
    }
}