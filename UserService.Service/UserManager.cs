using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.User;
using UserService.Model.Entities;
using UserService.Model.Exceptions;

namespace UserService.Service;

public class UserManager(IUserRepository userRepository, IMapper mapper, ILogger<UserManager> logger) : IUserManager
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UserManager> _logger = logger;
    public async Task<UserDTO> RegisterAsync(CreateUserDTO userDto, CancellationToken cancellationToken)
    {
        await ValidateUidAndEmail(Guid.Empty, userDto.Uid, userDto.Email, cancellationToken);
        ValidateDateOfBirth(userDto.DateOfBirth);
        var user = await _userRepository.AddAsync(_mapper.Map<User>(userDto), cancellationToken);
        _logger.LogInformation($"User with Id {user.Id} successfully registered");
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO> UpdateAsync(UpdateUserDTO userDto, Guid id, CancellationToken cancellationToken)
    {
        await ValidateUidAndEmail(id, userDto.Uid, userDto.Email, cancellationToken);
        ValidateDateOfBirth(userDto.DateOfBirth);
        var user = await _userRepository.GetAsync(id, cancellationToken);
        if (user != null)
        {
            _mapper.Map(userDto, user);
            user = await _userRepository.UpdateAsync(user, cancellationToken);
            if (user == null) UserNotFound(id);
        }
        else
        {
            UserNotFound(id);
        }
        _logger.LogInformation($"User with Id {id} successfully updated");
        return _mapper.Map<UserDTO>(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _userRepository.DeleteAsync(id, cancellationToken))
        {
            UserNotFound(id);
        }
        _logger.LogInformation($"User with Id {id} successfully deleted");
    }

    public async Task<UserDTO> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        if (user == null) UserNotFound(id);
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<ShortUserDTO?> GetShortAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        if (user == null) UserNotFound(id);
        return _mapper.Map<ShortUserDTO>(user);
    }

    private void UserNotFound(Guid id)
    {
        _logger.LogWarning($"DeleteAsync: User with Id {id} not found");
        throw new UserServiceException("Такого пользователя не существует.", 404);
    }
    
    private async Task ValidateUidAndEmail(Guid id, string? uid, string? email, CancellationToken cancellationToken)
    {
        if (uid != null && await _userRepository.ExistsWithUidAsync(id, uid, cancellationToken))
        {
            _logger.LogWarning($"Validation failed: Uid {uid} already exists");
            throw new UserServiceException("Пользователь с таким UID уже существует.", 409);
        }

        if (email != null && await _userRepository.ExistsWithEmailAsync(id, email, cancellationToken))
        {
            _logger.LogWarning($"Validation failed: Email {email} already exists");
            throw new UserServiceException("Этот адрес электронной почты уже привязан в другой учётной записи.", 409);
        }
    }

    private void ValidateDateOfBirth(DateOnly? dateOfBirth)
    {
        if (dateOfBirth.HasValue && dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            _logger.LogWarning("Registration failed: DateOfBirth is in the future");
            throw new UserServiceException("Ого! Вы гость из будущего! Но таких мы не регистрируем :(", 400);
        }
    }
}