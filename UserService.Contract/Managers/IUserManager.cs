using UserService.Model.DTO.User;
using UserService.Model.Entities;

namespace UserService.Contract.Managers;

public interface IUserManager
{
    public Task<UserDTO> RegisterAsync(CreateUserDTO userDto, CancellationToken ct);
    public Task<UserDTO> UpdateAsync(UpdateUserDTO userDto, Guid id, CancellationToken ct);
    public Task DeleteAsync(Guid id, CancellationToken ct);
    public Task<UserDTO> GetAsync(Guid id, CancellationToken ct);
    public Task<ShortUserDTO> GetShortAsync(Guid id, CancellationToken ct);
}