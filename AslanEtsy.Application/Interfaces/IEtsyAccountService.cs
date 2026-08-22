using AslanEtsy.Application.DTOs.Accounts;

namespace AslanEtsy.Application.Interfaces;

public interface IEtsyAccountService
{
    Task<IReadOnlyList<EtsyAccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default);
    Task<EtsyAccountDto?> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EtsyAccountDto> CreateAccountAsync(CreateEtsyAccountDto dto, CancellationToken cancellationToken = default);
    Task<EtsyAccountDto?> UpdateAccountAsync(int id, UpdateEtsyAccountDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAccountAsync(int id, CancellationToken cancellationToken = default);
    
    // OAuth flow
    Task<OAuthAuthorizeResultDto> InitiateOAuthAsync(int accountId, string redirectUri, CancellationToken cancellationToken = default);
    Task<bool> HandleOAuthCallbackAsync(string state, string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<bool> RefreshAccountTokenIfNeededAsync(int accountId, CancellationToken cancellationToken = default);
}
