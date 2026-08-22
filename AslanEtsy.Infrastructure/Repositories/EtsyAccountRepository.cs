using AslanEtsy.Domain.Entities;
using AslanEtsy.Domain.Interfaces;
using AslanEtsy.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AslanEtsy.Infrastructure.Repositories;

public class EtsyAccountRepository : Repository<EtsyAccount>, IEtsyAccountRepository
{
    public EtsyAccountRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<EtsyAccount?> GetByShopIdAsync(long shopId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.ShopId == shopId, cancellationToken);
    }

    public async Task<EtsyAccount?> GetByOAuthStateAsync(string state, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.OAuthState == state, cancellationToken);
    }

    public async Task<IReadOnlyList<EtsyAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(a => a.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<EtsyAccount?> GetAccountWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Orders)
            .Include(a => a.SyncLogs)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
