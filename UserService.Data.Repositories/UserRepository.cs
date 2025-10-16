using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;
using UserService.Model.Exceptions;

namespace UserService.Data.Repositories;

public class UserRepository(ILogger<UserRepository> logger, DataBaseContext context) : IUserRepository
{
    private readonly DataBaseContext _context = context;
    private readonly ILogger<UserRepository> _logger = logger;
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _context.Users.AddAsync(user, cancellationToken);
        await TrySaveChangeAsync("Add", user.Id, cancellationToken, "Ошибка базы данных при добавлении пользователя.");
        return user;
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existingUser = await _context.Users.FindAsync([user.Id], cancellationToken);
        if (existingUser == null) return null;
        _context.Entry(existingUser).CurrentValues.SetValues(user);
        await TrySaveChangeAsync("Update", user.Id, cancellationToken, "Ошибка базы данных при обновлении пользователя.");
        return existingUser;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _context.Users.FindAsync([id], cancellationToken);
        if (user == null) return false;
        _context.Users.Remove(user);
        await TrySaveChangeAsync("Delete", id, cancellationToken, "Ошибка базы данных при удалении пользователя.");
        return true;
    }

    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user ?? null;
    }

    public async Task<bool> ExistsWithUidAsync(Guid id, string uid, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking().AnyAsync(u => u.Uid == uid && u.Id != id, cancellationToken);
    }

    public async Task<bool> ExistsWithEmailAsync(Guid id, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking().AnyAsync(u => u.Email == email && u.Id != id, cancellationToken);
    }
    
    // Обёртка try-catch над SaveChangesAsync
    private async Task TrySaveChangeAsync(string methodName, Guid userId, CancellationToken cancellationToken, string error)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"User with Id {userId} successfully successfully {methodName}");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"{methodName}Async: Database error while processing user with Id {userId}");
            throw new UserServiceException(error, 500);
        }
    }
}