using UserService.Model.DTO.User;
using UserService.Model.Entities;

namespace UserService.Contract.Managers;

public interface IUserManager
{
    public Task<UserDTO?> RegisterAsync(CreateUserDTO userDto, CancellationToken cancellationToken);
    public Task<UserDTO?> UpdateAsync(UpdateUserDTO userDto, Guid id, CancellationToken cancellationToken);
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    public Task<UserDTO?> GetAsync(Guid id, CancellationToken cancellationToken);
    public Task<ShortUserDTO?> GetShortAsync(Guid id, CancellationToken cancellationToken);
}