using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Exceptions;
using UserService.Model.Utilities;
using UserService.Service.Extensions;

namespace UserService.Service.Managers;

public class UserManager(IUserRepository userRepository, ILogger<UserManager> logger) : IUserManager
{
    public async Task<UserDTO> RegisterAsync(CreateUserDTO userDto, CancellationToken ct)
    {
        await ValidateUidAndEmail(Guid.Empty, userDto.Uid, userDto.Email, ct);
        ValidateDateOfBirth(userDto.DateOfBirth);
        var user = await userRepository.AddAsync(userDto.ToUser(), ct);
        logger.LogInformation($"User with Id {user.Id} successfully registered");
        return user.ToUserDto();
    }

    public async Task<UserDTO> UpdateAsync(UpdateUserDTO userDto, Guid id, CancellationToken ct)
    {
        await ValidateUidAndEmail(id, userDto.Uid, userDto.Email, ct);
        ValidateDateOfBirth(userDto.DateOfBirth);
        var user = await userRepository.GetAsyncForUpdate(id, ct);
        if (user == null)
        {
            logger.LogWarning($"UserManager(Update): User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        user.UpdateFromDto(userDto);
        await userRepository.SaveChangesAsync(ct);
        return user.ToUserDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        if (!await userRepository.DeleteAsync(id, ct))
        {
            logger.LogWarning($"UserManager(Delete): User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        logger.LogInformation($"User with Id {id} successfully deleted");
    }

    public async Task<UserDTO> GetAsync(Guid id, CancellationToken ct) =>
        (await GetUserAsync(id, ct)).ToUserDto();

    public async Task<ShortUserDTO> GetShortAsync(Guid id, CancellationToken ct) =>
        (await GetUserAsync(id, ct)).ToShortUserDto();

    private async Task<User> GetUserAsync(Guid id, CancellationToken ct)
    {
        var user = await userRepository.GetAsync(id, ct);
        if (user == null)
        {
            logger.LogWarning($"UserManager(GetShort): User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        return user;
    }

    public async Task<ShortUserDTO> GetByUidAsync(string uid, CancellationToken ct)
    {
        var user = await userRepository.GetByUidAsync(uid, ct);
        if (user == null)
        {
            logger.LogWarning($"UserManager(GetByUid): User with Uid {Sanitizer.Sanitize(uid)} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        return user.ToShortUserDto();
    }

    public async Task<PagedUsersMainDTO> GetByNickNameAsync(string nickname, int offset, int limit,
        CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        var (users, total) = await userRepository.GetByNicknameAsync(nickname, offset, limit, ct);
        var usersDto = users.Select(user => user.ToShortUserDto());
        return new PagedUsersMainDTO(usersDto, offset, limit,total);
    }

    public async Task<PagedUsersMainDTO> GetAllShortAsync(int offset, int limit, CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        var (users, total) = await userRepository.GetAllAsync(offset, limit, ct);
        var shortUserDtos = users.Select(user => user.ToShortUserDto());
        return new PagedUsersMainDTO(shortUserDtos, offset, limit, total);
    }
    
    public async Task ExistsAsync(Guid userId, CancellationToken ct)
    {
        if (!await userRepository.ExistsAsync(userId, ct))
        {
            logger.LogWarning($"FriendManager: User with Id {userId} not found");
            throw new UserServiceException("Пользователь не существует.", 404);
        }
    }
    
    private async Task ValidateUidAndEmail(Guid id, string? uid, string? email, CancellationToken ct)
    {
        if (uid != null && await userRepository.ExistsWithUidAsync(id, uid, ct))
        {
            logger.LogWarning($"Validation failed: Uid {Sanitizer.Sanitize(uid)} already exists");
            throw new UserServiceException("Пользователь с таким UID уже существует.", 409);
        }

        if (email != null && await userRepository.ExistsWithEmailAsync(id, email, ct))
        {
            logger.LogWarning($"Validation failed: Email uniqueness constraint violated for user {id}");
            throw new UserServiceException("Этот адрес электронной почты уже привязан в другой учётной записи.", 409);
        }
    }

    private void ValidateDateOfBirth(DateOnly? dateOfBirth)
    {
        if (dateOfBirth.HasValue && dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            logger.LogWarning("Registration failed: DateOfBirth is in the future");
            throw new UserServiceException("Ого! Вы гость из будущего! Но таких мы не регистрируем :(", 400);
        }
    }
    
    private static void ValidatePagination(int offset, int limit)
    {
        if (offset < 0) throw new UserServiceException($"Offset не может быть отрицательным.", 400);
        if (limit <= 0) throw new UserServiceException($"Limit должен быть больше нуля.", 400);
        if (limit > 20) throw new UserServiceException($"Limit не может превышать 20.", 400);
    }
}