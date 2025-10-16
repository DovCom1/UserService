using UserService.Model.DTO.EnemyUser;

namespace UserService.Contract.Managers;

public interface IEnemyManager
{
    public Task<EnemyUserDTO> AddAsync(CreateEnemyUserDTO enemyUserDto, CancellationToken cancellationToken);
    public Task<bool> ExistsAsync(Guid userId, Guid enemyId, CancellationToken cancellationToken);
    public Task DeleteAsync(EnemyUserDTO enemyUserDto, CancellationToken cancellationToken);
    public Task<PagedEnemyResponseDTO> GetEnemiesAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken = default);
}