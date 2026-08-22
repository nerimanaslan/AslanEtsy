namespace AslanEtsy.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEtsyAccountRepository Accounts { get; }
    IOrderRepository Orders { get; }
    IRepository<Entities.OrderItem> OrderItems { get; }
    IRepository<Entities.OrderTracking> OrderTrackings { get; }
    IRepository<Entities.SyncLog> SyncLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
