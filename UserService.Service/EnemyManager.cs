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

public class EnemyManager(IEnemyRepository enemyRepository, IUserManager userManager, IFriendManager friendManager, IMapper mapper, ILogger<EnemyManager> logger) : IEnemyManager
{
    public async Task<EnemyUserDTO> AddAsync(CreateEnemyUserDTO enemyUserDto, CancellationToken ct)
    {
        if (enemyUserDto.UserId == enemyUserDto.EnemyId)
        {
            logger.LogWarning($"EnemyManager(Add): UserId {enemyUserDto.UserId} cannot add self as enemy");
            throw new UserServiceException("Нельзя добавить себя в список врагов.", 400);
        }
        if (await friendManager.IsPendingOrAcceptedFriendAsync(enemyUserDto.UserId, enemyUserDto.EnemyId, ct))
        {
            var dto = new DeleteFriendUserDTO(enemyUserDto.UserId, enemyUserDto.EnemyId);
            await friendManager.DeleteAsync(dto, ct);
        }
        if (await enemyRepository.IsEnemy(enemyUserDto.UserId, enemyUserDto.EnemyId, ct))
        {
            logger.LogWarning($"EnemyManager(Add): Enemy relationship between {enemyUserDto.UserId} and {enemyUserDto.EnemyId} already exists");
            throw new UserServiceException("Пользователь уже находится в списке врагов", 409);
        }
        var enemy = await enemyRepository.AddAsync(mapper.Map<EnemyUser>(enemyUserDto), ct);
        logger.LogInformation($"User with Id {enemyUserDto.UserId} successfully added User with Id {enemyUserDto.EnemyId} to enemy");
        return mapper.Map<EnemyUserDTO>(enemy);
    }

    public async Task<bool> IsEnemy(Guid userId, Guid enemyId, CancellationToken ct)
    {
        await userManager.ExistsAsync(userId, ct);
        await userManager.ExistsAsync(enemyId, ct);
        return await enemyRepository.IsEnemy(userId, enemyId, ct);
    }

    public async Task DeleteAsync(EnemyUserDTO enemyUserDto, CancellationToken ct)
    {
        await userManager.ExistsAsync(enemyUserDto.UserId, ct);
        await userManager.ExistsAsync(enemyUserDto.EnemyId, ct);
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
        await userManager.ExistsAsync(userId, ct);
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
    
    private static void ValidatePagination(int offset, int limit)
    {
        if (offset < 0) throw new UserServiceException($"Offset не может быть отрицательным.", 400);
        if (limit <= 0) throw new UserServiceException($"Limit должен быть больше нуля.", 400);
        if (limit > 20) throw new UserServiceException($"Limit не может превышать 20.", 400);
    }
}