namespace Core.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta un proceso de negocio completo dentro de UNA transacción de base de datos (ACID):
    /// si cualquier paso falla, se revierte todo. Incluye reintentos ante fallas transitorias.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}
