using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public interface IUserRepository
{
    /// <summary>Busca por username o email (AsNoTracking).</summary>
    Task<User?> GetByUsernameOrEmailAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>Usuarios ordenados por username (AsNoTracking).</summary>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Usuario por Id (AsNoTracking).</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Usuario con seguimiento de cambios.</summary>
    Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UsernameOrEmailExistsAsync(string username, string email, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    void Add(User user);
}
