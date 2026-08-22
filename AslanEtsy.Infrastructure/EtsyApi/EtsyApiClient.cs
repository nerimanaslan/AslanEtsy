using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AslanEtsy.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AslanEtsy.Infrastructure.EtsyApi;

public class EtsyApiClient : IEtsyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EtsyApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EtsyApiClient(HttpClient httpClient, ILogger<EtsyApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public string GenerateAuthorizationUrl(string keystring, string redirectUri, string state, string codeChallenge)
    {
        var scopes = "shops_r shops_w transactions_r transactions_w listings_r";
        var encodedScopes = Uri.EscapeDataString(scopes);
        var encodedRedirect = Uri.EscapeDataString(redirectUri);

        return $"https://www.etsy.com/oauth/connect?response_type=code&client_id={keystring}&redirect_uri={encodedRedirect}&scope={encodedScopes}&state={state}&code_challenge={codeChallenge}&code_challenge_method=S256";
    }

    public async Task<EtsyTokenResponse?> ExchangeCodeForTokenAsync(
        string keystring,
        string redirectUri,
        string code,
        string codeVerifier,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.etsy.com/v3/public/oauth/token";

        var body = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", keystring },
            { "redirect_uri", redirectUri },
            { "code", code },
            { "code_verifier", codeVerifier }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(body)
        };

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Etsy OAuth Token Exchange failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return new EtsyTokenResponse { error = response.StatusCode.ToString(), error_description = content };
            }

            return JsonSerializer.Deserialize<EtsyTokenResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Etsy token exchange");
            return new EtsyTokenResponse { error = "Exception", error_description = ex.Message };
        }
    }

    public async Task<EtsyTokenResponse?> RefreshTokenAsync(
        string keystring,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var url = "https://api.etsy.com/v3/public/oauth/token";

        var body = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", keystring },
            { "refresh_token", refreshToken }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(body)
        };

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Etsy Refresh Token failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }

            return JsonSerializer.Deserialize<EtsyTokenResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Etsy token refresh");
            return null;
        }
    }

    public async Task<EtsyShopResponse?> GetShopDetailsAsync(
        string keystring,
        string accessToken,
        long shopId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://openapi.etsy.com/v3/application/shops/{shopId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        SetAuthHeaders(request, keystring, accessToken);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetShopDetails failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }

            return JsonSerializer.Deserialize<EtsyShopResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shop details for shop ID {ShopId}", shopId);
            return null;
        }
    }

    public async Task<EtsyReceiptListResponse?> GetShopReceiptsAsync(
        string keystring,
        string accessToken,
        long shopId,
        bool? wasPaid = null,
        bool? wasShipped = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"limit={Math.Clamp(limit, 1, 100)}",
            $"offset={Math.Max(0, offset)}"
        };

        if (wasPaid.HasValue)
        {
            queryParams.Add($"was_paid={wasPaid.Value.ToString().ToLower()}");
        }

        if (wasShipped.HasValue)
        {
            queryParams.Add($"was_shipped={wasShipped.Value.ToString().ToLower()}");
        }

        var url = $"https://openapi.etsy.com/v3/application/shops/{shopId}/receipts?{string.Join("&", queryParams)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        SetAuthHeaders(request, keystring, accessToken);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GetShopReceipts failed with status {StatusCode}: {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Etsy API Hatası ({response.StatusCode}): {content}");
            }

            return JsonSerializer.Deserialize<EtsyReceiptListResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shop receipts for shop ID {ShopId}", shopId);
            throw;
        }
    }

    public async Task<EtsyReceiptResponse?> GetShopReceiptByIdAsync(
        string keystring,
        string accessToken,
        long shopId,
        long receiptId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://openapi.etsy.com/v3/application/shops/{shopId}/receipts/{receiptId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        SetAuthHeaders(request, keystring, accessToken);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GetShopReceiptById failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }

            return JsonSerializer.Deserialize<EtsyReceiptResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt {ReceiptId} for shop {ShopId}", receiptId, shopId);
            return null;
        }
    }

    public async Task<bool> CreateReceiptShipmentAsync(
        string keystring,
        string accessToken,
        long shopId,
        long receiptId,
        string trackingCode,
        string carrierName,
        bool sendBcc = false,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://openapi.etsy.com/v3/application/shops/{shopId}/receipts/{receiptId}/tracking";

        var body = new Dictionary<string, string>
        {
            { "tracking_code", trackingCode },
            { "carrier_name", carrierName },
            { "send_bcc", sendBcc ? "true" : "false" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(body)
        };
        SetAuthHeaders(request, keystring, accessToken);

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("CreateReceiptShipment failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tracking for receipt {ReceiptId} in shop {ShopId}", receiptId, shopId);
            return false;
        }
    }

    private static void SetAuthHeaders(HttpRequestMessage request, string keystring, string accessToken)
    {
        request.Headers.Add("x-api-key", keystring);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
