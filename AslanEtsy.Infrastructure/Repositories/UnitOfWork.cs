using AslanEtsy.Domain.Entities;
using AslanEtsy.Domain.Interfaces;
using AslanEtsy.Infrastructure.Context;

namespace AslanEtsy.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IEtsyAccountRepository? _accounts;
    private IOrderRepository? _orders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<OrderTracking>? _orderTrackings;
    private IRepository<SyncLog>? _syncLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IEtsyAccountRepository Accounts => _accounts ??= new EtsyAccountRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);
    public IRepository<OrderTracking> OrderTrackings => _orderTrackings ??= new Repository<OrderTracking>(_context);
    public IRepository<SyncLog> SyncLogs => _syncLogs ??= new Repository<SyncLog>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
