using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.EnemyUser;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Exceptions;
using UserService.Model.Utilities;

namespace UserService.Service;

public class EnemyManager(IEnemyRepository enemyRepository, IUserRepository userRepository ,IMapper mapper, ILogger<EnemyManager> logger) : IEnemyManager
{
    public async Task<EnemyUserDTO> AddAsync(CreateEnemyUserDTO enemyUserDto, CancellationToken cancellationToken)
    {
        await CheckUserExists(enemyUserDto.UserId, "AddAsync", cancellationToken);
        await CheckUserExists(enemyUserDto.EnemyId, "AddAsync", cancellationToken);
        if (enemyUserDto.UserId == enemyUserDto.EnemyId)
        {
            logger.LogWarning($"AddAsync: UserId {enemyUserDto.UserId} cannot add self as enemy");
            throw new UserServiceException("Нельзя добавить себя в список врагов.", 400);
        }
        if (await enemyRepository.ExistsAsync(enemyUserDto.UserId, enemyUserDto.EnemyId, cancellationToken))
        {
            logger.LogWarning($"AddAsync: Enemy relationship between {enemyUserDto.UserId} and {enemyUserDto.EnemyId} already exists");
            throw new UserServiceException("Пользователь уже находится в списке врагов", 409);
        }
        var enemy = await enemyRepository.AddAsync(mapper.Map<EnemyUser>(enemyUserDto), cancellationToken);
        logger.LogInformation($"User with Id {enemyUserDto.UserId} successfully added User with Id {enemyUserDto.EnemyId} to enemy");
        return mapper.Map<EnemyUserDTO>(enemy);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid enemyId, CancellationToken cancellationToken)
    {
        await CheckUserExists(userId, "ExistsAsync", cancellationToken);
        await CheckUserExists(enemyId, "ExistsAsync", cancellationToken);
        return await enemyRepository.ExistsAsync(userId, enemyId, cancellationToken);
    }

    public async Task DeleteAsync(EnemyUserDTO enemyUserDto, CancellationToken cancellationToken)
    {
        await CheckUserExists(enemyUserDto.UserId, "DeleteAsync", cancellationToken);
        await CheckUserExists(enemyUserDto.EnemyId, "DeleteAsync", cancellationToken);
        if (!await enemyRepository.DeleteAsync(mapper.Map<EnemyUser>(enemyUserDto), cancellationToken))
        {
            logger.LogWarning($"DeleteAsync: Enemy relationship with UserId {enemyUserDto.UserId} and EnemyId {enemyUserDto.EnemyId} not found");
            throw new UserServiceException("Пользователь не находится в списке ваших врагов", 404);
        }
        logger.LogInformation($"User with Id {enemyUserDto.UserId} successfully deleted User with Id {enemyUserDto.EnemyId} from enemies");
    }

    public async Task<PagedEnemyResponseDTO> GetEnemiesAsync(Guid userId, int offset, int limit,
        CancellationToken cancellationToken = default)
    {
        ValidatePagination(offset, limit);
        await CheckUserExists(userId, "GetEnemiesAsync", cancellationToken);
        var (enemies, total) = await enemyRepository.GetEnemiesAsync(userId, offset, limit, cancellationToken);
        var data = enemies.Select(enemy => new ShortUserDTO(
            Id: enemy.Id,
            Uid: enemy.Uid,
            Nickname: enemy.Nickname,
            AvatarUrl: enemy.AvatarUrl,
            Status: enemy.Status.GetDescription()
        )).ToList();
        return new PagedEnemyResponseDTO(data, offset, limit, total);
    }
    
    private async Task CheckUserExists(Guid userId, string methodName, CancellationToken cancellationToken)
    {
        if (!await userRepository.ExistsAsync(userId, cancellationToken))
        {
            logger.LogWarning($"{methodName}: User with Id {userId} not found");
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