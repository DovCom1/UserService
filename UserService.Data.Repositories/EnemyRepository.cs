using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;
using UserService.Model.Exceptions;

namespace UserService.Data.Repositories;

public class EnemyRepository(DataBaseContext context) : IEnemyRepository
{
    public async Task<EnemyUser> AddAsync(EnemyUser entity, CancellationToken cancellationToken = default)
    {
        await context.Enemies.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid enemyId, CancellationToken cancellationToken = default)
    {
        return await context.Enemies.AsNoTracking()
            .AnyAsync(e => e.UserId == userId && e.EnemyId == enemyId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(EnemyUser entity, CancellationToken cancellationToken = default)
    {
        var enemyUser =
            await context.Enemies.FindAsync([entity.UserId, entity.EnemyId], cancellationToken);
        if (enemyUser == null) return false;
        context.Enemies.Remove(enemyUser);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IEnumerable<User> enemies, int total)> GetEnemiesAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default)
    {
        var query = context.Enemies
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Enemy)
            .Select(e => e.Enemy)
            .OrderBy(u => u.Id)
            .Skip(offset)
            .Take(limit);
        var enemies = await query.ToListAsync(cancellationToken);
        var total = await context.Enemies.CountAsync(e => e.UserId == userId, cancellationToken);
        return (enemies, total);
        
    }
}