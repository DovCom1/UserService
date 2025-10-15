using UserService.Contract.Managers;
using UserService.Contract.Repositories;
using UserService.Model.DTO.User;

namespace UserService.Service;

public class UserManager(IUserRepository userRepository) : IUserManager
{
    private readonly IUserRepository _userRepository = userRepository;
    public async Task<UserDTO?> RegisterAsync(CreateUserDTO user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<UserDTO?> UpdateAsync(UpdateUserDTO user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await userRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<UserDTO?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<ShortUserDTO?> GetShortAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}