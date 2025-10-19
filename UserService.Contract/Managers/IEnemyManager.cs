using UserService.Model.DTO.EnemyUser;

namespace UserService.Contract.Managers;

public interface IEnemyManager
{
    public Task<EnemyUserDTO> AddAsync(CreateEnemyUserDTO enemyUserDto, CancellationToken ct);
    public Task<bool> ExistsAsync(Guid userId, Guid enemyId, CancellationToken ct);
    public Task DeleteAsync(EnemyUserDTO enemyUserDto, CancellationToken ct);
    public Task<PagedEnemyResponseDTO> GetEnemiesAsync(Guid userId, int offset, int limit,
        CancellationToken ct = default);
}