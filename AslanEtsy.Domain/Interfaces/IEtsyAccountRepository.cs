using AslanEtsy.Domain.Entities;

namespace AslanEtsy.Domain.Interfaces;

public interface IEtsyAccountRepository : IRepository<EtsyAccount>
{
    Task<EtsyAccount?> GetByShopIdAsync(long shopId, CancellationToken cancellationToken = default);
    Task<EtsyAccount?> GetByOAuthStateAsync(string state, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EtsyAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<EtsyAccount?> GetAccountWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
