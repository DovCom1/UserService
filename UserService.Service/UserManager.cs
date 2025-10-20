using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Enums;
using UserService.Model.Exceptions;
using UserService.Model.Utilities;

namespace UserService.Service;

public class UserManager(IUserRepository userRepository, IMapper mapper, ILogger<UserManager> logger) : IUserManager
{
    public async Task<UserDTO> RegisterAsync(CreateUserDTO userDto, CancellationToken ct)
    {
        await ValidateUidAndEmail(Guid.Empty, userDto.Uid, userDto.Email, ct);
        ValidateDateOfBirth(userDto.DateOfBirth);
        var user = await userRepository.AddAsync(mapper.Map<User>(userDto), ct);
        logger.LogInformation($"User with Id {user.Id} successfully registered");
        return mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO> UpdateAsync(UpdateUserDTO userDto, Guid id, CancellationToken ct)
    {
        await ValidateUidAndEmail(id, userDto.Uid, userDto.Email, ct);
        ValidateDateOfBirth(userDto.DateOfBirth);
        var user = await userRepository.GetAsyncForUpdate(id, ct);
        if (user == null)
        {
            logger.LogWarning($"DeleteAsync: User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        if (userDto.Uid != null) user.Uid = userDto.Uid;
        if (userDto.Nickname != null) user.Nickname = userDto.Nickname;
        if (userDto.Email != null) user.Email = userDto.Email;
        if (userDto.AvatarUrl != null) user.AvatarUrl = userDto.AvatarUrl;
        if (userDto.Gender != null) user.Gender = userDto.Gender.ParseByDescription<Gender>();
        if (userDto.Status != null) user.Status = userDto.Status.ParseByDescription<UserStatus>();
        if (userDto.DateOfBirth != null) user.DateOfBirth = userDto.DateOfBirth.Value;
        await userRepository.SaveChangesAsync(ct);
        return mapper.Map<UserDTO>(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        if (!await userRepository.DeleteAsync(id, ct))
        {
            logger.LogWarning($"DeleteAsync: User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        logger.LogInformation($"User with Id {id} successfully deleted");
    }

    public async Task<UserDTO> GetAsync(Guid id, CancellationToken ct)
    {
        var user = await userRepository.GetAsync(id, ct);
        if (user == null)
        {
            logger.LogWarning($"DeleteAsync: User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        return mapper.Map<UserDTO>(user);
    }

    public async Task<ShortUserDTO> GetShortAsync(Guid id, CancellationToken ct)
    {
        var user = await userRepository.GetAsync(id, ct);
        if (user == null)
        {
            logger.LogWarning($"DeleteAsync: User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        return mapper.Map<ShortUserDTO>(user);
    }

    public async Task<ShortUserDTO> GetByUidAsync(string uid, CancellationToken ct)
    {
        var user = await userRepository.GetByUidAsync(uid, ct);
        if (user == null)
        {
            logger.LogWarning($"DeleteAsync: User with Uid {uid} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        return mapper.Map<ShortUserDTO>(user);
    }

    public async Task<PagedUsersMainDTO> GetByNickNameAsync(string nickname, int offset, int limit,
        CancellationToken ct)
    {
        ValidatePagination(offset, limit);
        var (users, total) = await userRepository.GetByNicknameAsync(nickname, offset, limit, ct);
        var usersDto = users.Select(user => mapper.Map<ShortUserDTO>(user));
        return new PagedUsersMainDTO(usersDto, offset, limit,total);
    }
    
    public async Task<PagedUsersDTO> GetAllAsync(int offset, int limit, CancellationToken ct = default)
    {
        ValidatePagination(offset, limit);
        var (users, total) = await userRepository.GetAllAsync(offset, limit, ct);
        var userDtos = users.Select(user => mapper.Map<UserDTO>(user));
        return new PagedUsersDTO(userDtos, offset, limit, total);
    }

    public async Task<PagedUsersMainDTO> GetAllShortAsync(int offset, int limit, CancellationToken ct = default)
    {
        ValidatePagination(offset, limit);
        var (users, total) = await userRepository.GetAllAsync(offset, limit, ct);
        var shortUserDtos = users.Select(user => mapper.Map<ShortUserDTO>(user));
        return new PagedUsersMainDTO(shortUserDtos, offset, limit, total);
    }
    
    private async Task ValidateUidAndEmail(Guid id, string? uid, string? email, CancellationToken ct)
    {
        if (uid != null && await userRepository.ExistsWithUidAsync(id, uid, ct))
        {
            logger.LogWarning($"Validation failed: Uid {uid} already exists");
            throw new UserServiceException("Пользователь с таким UID уже существует.", 409);
        }

        if (email != null && await userRepository.ExistsWithEmailAsync(id, email, ct))
        {
            logger.LogWarning($"Validation failed: Email {email} already exists");
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