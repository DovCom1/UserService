using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.EnemyUser;
using UserService.Model.DTO.FriendUser;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Exceptions;
using UserService.Model.Utilities;

namespace UserService.Service;

public class EnemyManager(IEnemyRepository enemyRepository, IUserRepository userRepository, IFriendRepository friendRepository, IMapper mapper, ILogger<EnemyManager> logger) : IEnemyManager
{
    public async Task<EnemyUserDTO> AddAsync(CreateEnemyUserDTO enemyUserDto, CancellationToken ct)
    {
        await CheckUserExists(enemyUserDto.UserId, ct);
        await CheckUserExists(enemyUserDto.EnemyId, ct);
        if (enemyUserDto.UserId == enemyUserDto.EnemyId)
        {
            logger.LogWarning($"EnemyManager(Add): UserId {enemyUserDto.UserId} cannot add self as enemy");
            throw new UserServiceException("Нельзя добавить себя в список врагов.", 400);
        }
        if (await enemyRepository.ExistsAsync(enemyUserDto.UserId, enemyUserDto.EnemyId, ct))
        {
            logger.LogWarning($"EnemyManager(Add): Enemy relationship between {enemyUserDto.UserId} and {enemyUserDto.EnemyId} already exists");
            throw new UserServiceException("Пользователь уже находится в списке врагов", 409);
        }
        if (await friendRepository.ExistsAsync(enemyUserDto.UserId, enemyUserDto.EnemyId, ct))
        {
            var dto = new DeleteFriendUserDTO(enemyUserDto.UserId, enemyUserDto.EnemyId);
            await friendRepository.DeleteAsync(mapper.Map<FriendUser>(dto), ct);
        }
        var enemy = await enemyRepository.AddAsync(mapper.Map<EnemyUser>(enemyUserDto), ct);
        logger.LogInformation($"User with Id {enemyUserDto.UserId} successfully added User with Id {enemyUserDto.EnemyId} to enemy");
        return mapper.Map<EnemyUserDTO>(enemy);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid enemyId, CancellationToken ct)
    {
        await CheckUserExists(userId, ct);
        await CheckUserExists(enemyId, ct);
        return await enemyRepository.ExistsAsync(userId, enemyId, ct);
    }

    public async Task DeleteAsync(EnemyUserDTO enemyUserDto, CancellationToken ct)
    {
        await CheckUserExists(enemyUserDto.UserId, ct);
        await CheckUserExists(enemyUserDto.EnemyId, ct);
        if (!await enemyRepository.DeleteAsync(mapper.Map<EnemyUser>(enemyUserDto), ct))
        {
            logger.LogWarning($"EnemyManager(Delete): Enemy relationship with UserId {enemyUserDto.UserId} and EnemyId {enemyUserDto.EnemyId} not found");
            throw new UserServiceException("Пользователь не находится в списке ваших врагов", 404);
        }
        logger.LogInformation($"User with Id {enemyUserDto.UserId} successfully deleted User with Id {enemyUserDto.EnemyId} from enemies");
    }

    public async Task<PagedEnemyResponseDTO> GetEnemiesAsync(Guid userId, int offset, int limit,
        CancellationToken ct = default)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, ct);
        var (enemies, total) = await enemyRepository.GetEnemiesAsync(userId, offset, limit, ct);
        var data = enemies.Select(enemy => new ShortUserDTO(
            Id: enemy.Id,
            Uid: enemy.Uid,
            Nickname: enemy.Nickname,
            AvatarUrl: enemy.AvatarUrl,
            Status: enemy.Status.GetDescription()
        )).ToList();
        return new PagedEnemyResponseDTO(data, offset, limit, total);
    }
    
    private async Task CheckUserExists(Guid userId, CancellationToken ct)
    {
        if (!await userRepository.ExistsAsync(userId, ct))
        {
            logger.LogWarning($"EnemyManager: User with Id {userId} not found");
            throw new UserServiceException("Пользователь не существует.", 404);
        }
    }
    
    private static void ValidatePagination(int offset, int limit)
    {
        if (offset < 0) throw new UserServiceException($"Offset не может быть отрицательным.", 400);
        if (limit <= 0) throw new UserServiceException($"Limit должен быть больше нуля.", 400);
        if (limit > 20) throw new UserServiceException($"Limit не может превышать 20.", 400);
    }
}