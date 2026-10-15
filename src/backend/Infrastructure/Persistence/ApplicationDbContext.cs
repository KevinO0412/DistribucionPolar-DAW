using System.Reflection;
using Core.Application.Abstractions;
using Core.Domain.Entities;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Truck> Trucks => Set<Truck>();

    public DbSet<TruckStock> TruckStocks => Set<TruckStock>();

    public DbSet<TruckLoad> TruckLoads => Set<TruckLoad>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<ClientEmptyBalance> ClientEmptyBalances => Set<ClientEmptyBalance>();

    public DbSet<ClientMovement> ClientMovements => Set<ClientMovement>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    public DbSet<InvoiceEmptyGroup> InvoiceEmptyGroups => Set<InvoiceEmptyGroup>();

    public DbSet<Purchase> Purchases => Set<Purchase>();

    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

    public DbSet<DamagedProduct> DamagedProducts => Set<DamagedProduct>();

    public DbSet<InternalConsumption> InternalConsumptions => Set<InternalConsumption>();

    public DbSet<ConsignmentEvent> ConsignmentEvents => Set<ConsignmentEvent>();

    public DbSet<ConsignmentItem> ConsignmentItems => Set<ConsignmentItem>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<DocumentCounter> DocumentCounters => Set<DocumentCounter>();

    /// <summary>
    /// Ejecuta el proceso dentro de una transacción explícita (BEGIN … COMMIT). La estrategia de ejecución
    /// de Npgsql reintenta el bloque completo ante fallas transitorias de conexión; antes de cada intento
    /// se limpia el ChangeTracker para no arrastrar cambios del intento fallido.
    /// </summary>
    public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            ChangeTracker.Clear();
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
