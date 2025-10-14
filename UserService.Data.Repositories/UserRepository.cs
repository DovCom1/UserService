using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;

namespace UserService.Data.Repositories;

public class UserRepository(ILogger<UserRepository> logger, DataBaseContext context) : IUserRepository
{
    private readonly DataBaseContext _context = context;
    private readonly ILogger<UserRepository> _logger = logger;
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entry = _context.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            _logger.LogWarning($"UpdateAsync: User with ID {user.Id} is not tracked");
            return null;
        }
        await _context.SaveChangesAsync(cancellationToken);
        await entry.ReloadAsync(cancellationToken);
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _context.Users.FindAsync(new object[] {id}, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("DeleteAsync: User with id: {id} not found", id);
            return false;
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _context.Users.FindAsync(new object[] {id}, cancellationToken);
        if (user != null) return user;
        _logger.LogWarning("GetAsync: User with id: {id} not found", id);
        return null;
    }
}