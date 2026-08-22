namespace AslanEtsy.Application.Interfaces;

public class EtsyTokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public string refresh_token { get; set; } = string.Empty;
    public string? error { get; set; }
    public string? error_description { get; set; }
}

public class EtsyShopResponse
{
    public long shop_id { get; set; }
    public long user_id { get; set; }
    public string shop_name { get; set; } = string.Empty;
    public string? url { get; set; }
    public string? icon_url_fullxfull { get; set; }
    public string? currency_code { get; set; }
    public int listing_active_count { get; set; }
}

public class EtsyReceiptListResponse
{
    public int count { get; set; }
    public List<EtsyReceiptResponse> results { get; set; } = new();
}

public class EtsyReceiptResponse
{
    public long receipt_id { get; set; }
    public long receipt_type { get; set; }
    public long seller_user_id { get; set; }
    public long buyer_user_id { get; set; }
    public string? buyer_email { get; set; }
    public string? name { get; set; }
    public string? first_line { get; set; }
    public string? second_line { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? zip { get; set; }
    public string? country_iso { get; set; }
    public string? formatted_address { get; set; }
    public string? message_from_buyer { get; set; }
    public string? message_from_payment { get; set; }
    public bool is_paid { get; set; }
    public bool is_shipped { get; set; }
    public long create_timestamp { get; set; }
    public long created_timestamp { get; set; }
    public long update_timestamp { get; set; }
    public long updated_timestamp { get; set; }
    public bool is_gift { get; set; }
    public string? gift_message { get; set; }
    public EtsyAmount? grandtotal { get; set; }
    public EtsyAmount? subtotal { get; set; }
    public EtsyAmount? total_price { get; set; }
    public EtsyAmount? total_shipping_cost { get; set; }
    public EtsyAmount? total_tax_cost { get; set; }
    public EtsyAmount? discount_amt { get; set; }
    public List<EtsyTransactionResponse> transactions { get; set; } = new();
    public List<EtsyShipmentResponse> shipments { get; set; } = new();
}

public class EtsyAmount
{
    public int amount { get; set; }
    public int divisor { get; set; } = 100;
    public string currency_code { get; set; } = "USD";
    public decimal DecimalValue => divisor == 0 ? amount : (decimal)amount / divisor;
}

public class EtsyTransactionResponse
{
    public long transaction_id { get; set; }
    public string? title { get; set; }
    public string? description { get; set; }
    public long listing_id { get; set; }
    public int quantity { get; set; }
    public EtsyAmount? price { get; set; }
    public string? sku { get; set; }
    public List<EtsyVariationValueResponse>? variations { get; set; }
    public string? buyer_personalization { get; set; }
    public long? main_image_listing_id { get; set; }
    public string? image_url { get; set; }
}

public class EtsyVariationValueResponse
{
    public long property_id { get; set; }
    public string? formatted_name { get; set; }
    public long? value_id { get; set; }
    public string? formatted_value { get; set; }
}

public class EtsyShipmentResponse
{
    public long receipt_shipping_id { get; set; }
    public string? tracking_code { get; set; }
    public string? carrier_name { get; set; }
    public long ship_date { get; set; }
}

public interface IEtsyApiClient
{
    string GenerateAuthorizationUrl(string keystring, string redirectUri, string state, string codeChallenge);
    Task<EtsyTokenResponse?> ExchangeCodeForTokenAsync(string keystring, string redirectUri, string code, string codeVerifier, CancellationToken cancellationToken = default);
    Task<EtsyTokenResponse?> RefreshTokenAsync(string keystring, string refreshToken, CancellationToken cancellationToken = default);
    Task<EtsyShopResponse?> GetShopDetailsAsync(string keystring, string accessToken, long shopId, CancellationToken cancellationToken = default);
    Task<EtsyReceiptListResponse?> GetShopReceiptsAsync(string keystring, string accessToken, long shopId, bool? wasPaid = null, bool? wasShipped = null, int limit = 100, int offset = 0, CancellationToken cancellationToken = default);
    Task<EtsyReceiptResponse?> GetShopReceiptByIdAsync(string keystring, string accessToken, long shopId, long receiptId, CancellationToken cancellationToken = default);
    Task<bool> CreateReceiptShipmentAsync(string keystring, string accessToken, long shopId, long receiptId, string trackingCode, string carrierName, bool sendBcc = false, CancellationToken cancellationToken = default);
}
