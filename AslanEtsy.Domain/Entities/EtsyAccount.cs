using AslanEtsy.Domain.Common;

namespace AslanEtsy.Domain.Entities;

public class EtsyAccount : BaseEntity
{
    public long ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? ShopUrl { get; set; }
    public string? IconUrl { get; set; }
    
    // Etsy OAuth 2.0 Credentials & Tokens
    public string Keystring { get; set; } = string.Empty; // Client ID / Keystring
    public string? SharedSecret { get; set; } // Client Secret (if applicable)
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAtUtc { get; set; }
    
    // PKCE state temporary storage during authorization flow
    public string? CodeVerifier { get; set; }
    public string? OAuthState { get; set; }

    public bool IsActive { get; set; } = true;
    public bool AutoSyncEnabled { get; set; } = true;
    public DateTime? LastSyncAtUtc { get; set; }
    public string? LastSyncError { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<SyncLog> SyncLogs { get; set; } = new List<SyncLog>();
}
