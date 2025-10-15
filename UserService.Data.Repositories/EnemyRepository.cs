using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;

namespace UserService.Data.Repositories;

public class EnemyRepository(ILogger<EnemyRepository> logger, DataBaseContext context) : IEnemyRepository
{
    private readonly DataBaseContext _context = context;
    private readonly ILogger<EnemyRepository> _logger = logger;

    public async Task<EnemyUser> AddAsync(EnemyUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _context.Enemies.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> ExistsAsync(EnemyUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var enemyUser =
            await _context.Enemies.FindAsync([entity.UserId, entity.EnemyId], cancellationToken);
        return enemyUser != null;
    }

    public async Task<bool> DeleteAsync(EnemyUser entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var enemyUser =
            await _context.Enemies.FindAsync([entity.UserId, entity.EnemyId], cancellationToken);
        if (enemyUser == null)
        {
            _logger.LogWarning($"DeleteAsync: Enemy relationship with UserId {entity.UserId} and EnemyId {entity.EnemyId} not found");
            return false;
        }
        _context.Enemies.Remove(enemyUser);
        await _context.SaveChangesAsync(cancellationToken);
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
        var enemies = await query.ToListAsync(cancellationToken);
        var total = await _context.Enemies.CountAsync(e => e.UserId == userId,cancellationToken);
        return (enemies, total);
    }
}