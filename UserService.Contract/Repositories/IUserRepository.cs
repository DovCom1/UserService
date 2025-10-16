using UserService.Model.Entities;

namespace UserService.Contract.Repositories;

public interface IUserRepository
{
    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<bool> ExistsWithUidAsync(Guid id, string uid, CancellationToken cancellationToken = default);
    public Task<bool> ExistsWithEmailAsync(Guid id, string email, CancellationToken cancellationToken = default);
}