namespace AslanEtsy.Application.DTOs.Accounts;

public class EtsyAccountDto
{
    public int Id { get; set; }
    public long ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? ShopUrl { get; set; }
    public string? IconUrl { get; set; }
    public string Keystring { get; set; } = string.Empty;
    public bool IsConnected => !string.IsNullOrWhiteSpace(AccessToken) && (TokenExpiresAtUtc == null || TokenExpiresAtUtc > DateTime.UtcNow);
    public bool IsActive { get; set; }
    public bool AutoSyncEnabled { get; set; }
    public DateTime? LastSyncAtUtc { get; set; }
    public string? LastSyncError { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int OrderCount { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? TokenExpiresAtUtc { get; set; }
}

public class CreateEtsyAccountDto
{
    public string ShopName { get; set; } = string.Empty;
    public long ShopId { get; set; }
    public string Keystring { get; set; } = string.Empty; // Client ID / Keystring
    public string? SharedSecret { get; set; }
    public bool AutoSyncEnabled { get; set; } = true;
}

public class UpdateEtsyAccountDto
{
    public string ShopName { get; set; } = string.Empty;
    public string Keystring { get; set; } = string.Empty;
    public string? SharedSecret { get; set; }
    public bool IsActive { get; set; }
    public bool AutoSyncEnabled { get; set; }
}

public class OAuthAuthorizeResultDto
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CodeVerifier { get; set; } = string.Empty;
}

public class OAuthCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
