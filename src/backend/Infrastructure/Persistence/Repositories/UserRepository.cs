using Core.Application.Abstractions;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<User?> GetByUsernameOrEmailAsync(string login, CancellationToken cancellationToken = default)
    {
        return context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == login || u.Email == login, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Users.AsNoTracking().OrderBy(u => u.Username).ToListAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<bool> UsernameOrEmailExistsAsync(string username, string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return context.Users
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Id != excludeId && (u.Username == username || u.Email == email), cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return context.Users.AsNoTracking().AnyAsync(cancellationToken);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }
}
