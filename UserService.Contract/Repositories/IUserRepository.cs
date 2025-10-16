using UserService.Model.Entities;

namespace UserService.Contract.Repositories;

public interface IUserRepository
{
    public Task<User> AddAsync(User user, CancellationToken ct = default);
    public Task SaveChangesAsync(CancellationToken ct = default);
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    public Task<User?> GetAsync(Guid id, CancellationToken ct = default);
    public Task<User?> GetAsyncForUpdate(Guid id, CancellationToken ct = default);
    public Task<bool> ExistsWithUidAsync(Guid id, string uid, CancellationToken ct = default);
    public Task<bool> ExistsWithEmailAsync(Guid id, string email, CancellationToken ct = default);
    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}