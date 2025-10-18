using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Contract.Repositories;
using UserService.Data.Core;
using UserService.Model.Entities;
using UserService.Model.Exceptions;

namespace UserService.Data.Repositories;

public class UserRepository(DataBaseContext context) : IUserRepository
{
    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
        return user;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
       var user = await context.Users.FindAsync([id], ct);
        if (user == null) return false;
        context.Users.Remove(user);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<User?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        return user;
    }

    public async Task<(IEnumerable<User> users, int total)> GetAllAsync(int offset, int limit, CancellationToken ct = default)
    {
        var query = context.Users.AsNoTracking();
        var total = await query.CountAsync(ct);
        var users = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);
        return (users, total);
    }

    public async Task<User?> GetAsyncForUpdate(Guid id, CancellationToken ct = default)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<bool> ExistsWithUidAsync(Guid id, string uid, CancellationToken ct = default)
    {
        return await context.Users.AsNoTracking().AnyAsync(u => u.Uid == uid && u.Id != id, ct);
    }

    public async Task<bool> ExistsWithEmailAsync(Guid id, string email, CancellationToken ct = default)
    {
        return await context.Users.AsNoTracking().AnyAsync(u => u.Email == email && u.Id != id, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Users.AsNoTracking().AnyAsync(u => u.Id == id, ct);
    }
    
}