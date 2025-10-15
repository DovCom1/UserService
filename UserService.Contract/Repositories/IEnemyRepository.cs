using UserService.Model.Entities;

namespace UserService.Contract.Repositories;

public interface IEnemyRepository
{
    public Task<EnemyUser> AddAsync(EnemyUser entity, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(EnemyUser entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(EnemyUser entity, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<User> enemies, int total)> GetEnemiesAsync(Guid userId, int offset, int limit, CancellationToken cancellationToken = default); 
}