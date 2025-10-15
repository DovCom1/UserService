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
        await ValidateUidAndEmail(user, cancellationToken);
        await _context.Users.AddAsync(user, cancellationToken);
        await TrySaveChangeAsync("Add", user.Id, cancellationToken, "Ошибка базы данных при добавлении пользователя.");
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ValidateUidAndEmail(user, cancellationToken);
        var existingUser = await _context.Users.FindAsync([user.Id], cancellationToken);
        if (existingUser == null)
        {
            _logger.LogWarning($"UpdateAsync: User with Id {user.Id} not found");
            throw new UserServiceException("Пользователь не найден.", 404);
        }
        _context.Entry(existingUser).CurrentValues.SetValues(user);
        await TrySaveChangeAsync("Update", user.Id, cancellationToken, "Ошибка базы данных при обновлении пользователя.");
        return existingUser;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _context.Users.FindAsync([id], cancellationToken);
        if (user == null)
        {
            _logger.LogWarning($"DeleteAsync: User with Id {id} not found");
            throw new UserServiceException("Такого пользователя не существует.", 404);
        }
        _context.Users.Remove(user);
        await TrySaveChangeAsync("Delete", id, cancellationToken, "Ошибка базы данных при удалении пользователя.");
    }

    public async Task<User> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user != null) return user;
        _logger.LogWarning($"GetAsync: User with id: {id} not found");
        throw new UserServiceException("Такого пользователя не существует.", 404);
    }
    
    private async Task ValidateUidAndEmail(User user, CancellationToken cancellationToken)
    {
        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Uid == user.Uid, cancellationToken))
        {
            throw new UserServiceException("Пользователь с таким UID уже существует.", 409);
        }
        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Email == user.Email, cancellationToken))
        {
            throw new UserServiceException("Этот адрес электронной почты уже привязан в другой учётной записи.", 409);
        }
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