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
        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Uid == user.Uid, cancellationToken))
        {
            throw new UserServiceException("Пользователь с таким UID уже существует.", 409);
        }
        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Email == user.Email, cancellationToken))
        {
            throw new UserServiceException("Этот адрес электронной почты уже привязан в другой учётной записи.", 409);
        }

        try
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"User with Id {user.Id} successfully added");
            return user;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "AddAsync: Database error while adding user");
            throw new UserServiceException("Ошибка базы данных при добавлении пользователя.", 500);
        }
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Uid == user.Uid && u.Id != user.Id, cancellationToken))
        {
            throw new UserServiceException("Пользователь с таким UID уже существует.", 409);
        }
        if (await _context.Users.AsNoTracking().AnyAsync(u => u.Email == user.Email && u.Id != user.Id, cancellationToken))
        {
            throw new UserServiceException("Этот адрес электронной почты уже привязан в другой учётной записи.", 409);
        }
        var existingUser = await _context.Users.FindAsync([user.Id], cancellationToken);
        if (existingUser == null)
        {
            _logger.LogWarning($"UpdateAsync: User with Id {user.Id} not found");
            throw new UserServiceException("Пользователь не найден.", 404);
        }
        
        _context.Entry(existingUser).CurrentValues.SetValues(user);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"User with Id Id {user.Id} successfully updated");
            return existingUser;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"Database error while updating user with Id {user.Id}");
            throw new UserServiceException("Ошибка базы данных при обновлении пользователя.", 500);
        }
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
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"User with ID {id} successfully deleted");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"Database error while deleting user with Id {id}");
            throw new UserServiceException("Ошибка базы данных при удалении пользователя.", 500);
        }
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
}