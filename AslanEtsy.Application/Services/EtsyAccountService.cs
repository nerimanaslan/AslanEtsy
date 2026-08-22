using System.Security.Cryptography;
using System.Text;
using AslanEtsy.Application.DTOs.Accounts;
using AslanEtsy.Application.Interfaces;
using AslanEtsy.Domain.Entities;
using AslanEtsy.Domain.Interfaces;

namespace AslanEtsy.Application.Services;

public class EtsyAccountService : IEtsyAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEtsyApiClient _etsyApiClient;

    public EtsyAccountService(IUnitOfWork unitOfWork, IEtsyApiClient etsyApiClient)
    {
        _unitOfWork = unitOfWork;
        _etsyApiClient = etsyApiClient;
    }

    public async Task<IReadOnlyList<EtsyAccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _unitOfWork.Accounts.GetAllAsync(cancellationToken);
        return accounts.Select(MapToDto).ToList();
    }

    public async Task<EtsyAccountDto?> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Accounts.GetAccountWithDetailsAsync(id, cancellationToken);
        return account != null ? MapToDto(account) : null;
    }

    public async Task<EtsyAccountDto> CreateAccountAsync(CreateEtsyAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = new EtsyAccount
        {
            ShopId = dto.ShopId,
            ShopName = dto.ShopName,
            Keystring = dto.Keystring.Trim(),
            SharedSecret = dto.SharedSecret?.Trim(),
            AutoSyncEnabled = dto.AutoSyncEnabled,
            IsActive = true
        };

        await _unitOfWork.Accounts.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(account);
    }

    public async Task<EtsyAccountDto?> UpdateAccountAsync(int id, UpdateEtsyAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, cancellationToken);
        if (account == null) return null;

        account.ShopName = dto.ShopName;
        account.Keystring = dto.Keystring.Trim();
        account.SharedSecret = dto.SharedSecret?.Trim();
        account.IsActive = dto.IsActive;
        account.AutoSyncEnabled = dto.AutoSyncEnabled;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(account);
    }

    public async Task<bool> DeleteAccountAsync(int id, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, cancellationToken);
        if (account == null) return false;

        account.IsDeleted = true;
        account.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OAuthAuthorizeResultDto> InitiateOAuthAsync(int accountId, string redirectUri, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Mağaza bulunamadı (ID: {accountId})");

        // PKCE Code Verifier and Code Challenge generation
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = Guid.NewGuid().ToString("N");

        account.CodeVerifier = codeVerifier;
        account.OAuthState = state;
        account.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var authUrl = _etsyApiClient.GenerateAuthorizationUrl(account.Keystring, redirectUri, state, codeChallenge);

        return new OAuthAuthorizeResultDto
        {
            AuthorizationUrl = authUrl,
            State = state,
            CodeVerifier = codeVerifier
        };
    }

    public async Task<bool> HandleOAuthCallbackAsync(string state, string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Accounts.GetByOAuthStateAsync(state, cancellationToken);
        if (account == null || string.IsNullOrWhiteSpace(account.CodeVerifier))
            return false;

        var tokenResponse = await _etsyApiClient.ExchangeCodeForTokenAsync(
            account.Keystring,
            redirectUri,
            code,
            account.CodeVerifier,
            cancellationToken);

        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.access_token))
            return false;

        account.AccessToken = tokenResponse.access_token;
        account.RefreshToken = tokenResponse.refresh_token;
        account.TokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in - 60); // 1 min buffer
        account.CodeVerifier = null;
        account.OAuthState = null;
        account.UpdatedAtUtc = DateTime.UtcNow;

        // Optionally fetch shop profile to get up to date shop name and icon
        if (account.ShopId > 0)
        {
            try
            {
                var shopInfo = await _etsyApiClient.GetShopDetailsAsync(account.Keystring, account.AccessToken, account.ShopId, cancellationToken);
                if (shopInfo != null)
                {
                    account.ShopName = !string.IsNullOrEmpty(shopInfo.shop_name) ? shopInfo.shop_name : account.ShopName;
                    account.ShopUrl = shopInfo.url;
                    account.IconUrl = shopInfo.icon_url_fullxfull;
                }
            }
            catch
            {
                // Silently continue if shop metadata fetch fails
            }
        }

        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RefreshAccountTokenIfNeededAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);
        if (account == null || string.IsNullOrWhiteSpace(account.RefreshToken))
            return false;

        // If expires in less than 5 minutes or already expired
        if (account.TokenExpiresAtUtc.HasValue && account.TokenExpiresAtUtc.Value > DateTime.UtcNow.AddMinutes(5))
            return true;

        var tokenResponse = await _etsyApiClient.RefreshTokenAsync(account.Keystring, account.RefreshToken, cancellationToken);
        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.access_token))
            return false;

        account.AccessToken = tokenResponse.access_token;
        account.RefreshToken = tokenResponse.refresh_token;
        account.TokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in - 60);
        account.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncode(challengeBytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        return output;
    }

    private static EtsyAccountDto MapToDto(EtsyAccount account)
    {
        return new EtsyAccountDto
        {
            Id = account.Id,
            ShopId = account.ShopId,
            ShopName = account.ShopName,
            ShopUrl = account.ShopUrl,
            IconUrl = account.IconUrl,
            Keystring = account.Keystring,
            IsActive = account.IsActive,
            AutoSyncEnabled = account.AutoSyncEnabled,
            LastSyncAtUtc = account.LastSyncAtUtc,
            LastSyncError = account.LastSyncError,
            CreatedAtUtc = account.CreatedAtUtc,
            OrderCount = account.Orders?.Count ?? 0,
            AccessToken = account.AccessToken,
            TokenExpiresAtUtc = account.TokenExpiresAtUtc
        };
    }
}
