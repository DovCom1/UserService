using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.User;
using UserService.Model.Entities;

namespace UserService.Service;

public class UserManager(IUserRepository userRepository, IMapper mapper, ILogger<UserManager> logger) : IUserManager
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UserManager> _logger = logger;
    public async Task<UserDTO?> RegisterAsync(CreateUserDTO userDto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.AddAsync(_mapper.Map<User>(userDto), cancellationToken);
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO?> UpdateAsync(UpdateUserDTO userDto, Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        if (user == null) return null;
        _mapper.Map(userDto, user);
        await _userRepository.UpdateAsync(user, cancellationToken);
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _userRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<UserDTO?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        return user == null ? null : _mapper.Map<UserDTO>(user);
    }

    public async Task<ShortUserDTO?> GetShortAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        return user == null ? null : _mapper.Map<ShortUserDTO>(user);
    }
}