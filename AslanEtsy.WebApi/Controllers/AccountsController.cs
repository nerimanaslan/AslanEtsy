using AslanEtsy.Application.DTOs.Accounts;
using AslanEtsy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AslanEtsy.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IEtsyAccountService _accountService;

    public AccountsController(IEtsyAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EtsyAccountDto>>> GetAll(CancellationToken cancellationToken)
    {
        var accounts = await _accountService.GetAllAccountsAsync(cancellationToken);
        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EtsyAccountDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var account = await _accountService.GetAccountByIdAsync(id, cancellationToken);
        if (account == null) return NotFound(new { message = $"Mağaza bulunamadı (ID: {id})" });
        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<EtsyAccountDto>> Create([FromBody] CreateEtsyAccountDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.ShopName) || string.IsNullOrWhiteSpace(dto.Keystring))
        {
            return BadRequest(new { message = "Mağaza adı ve API Keystring (Client ID) alanları zorunludur." });
        }

        var account = await _accountService.CreateAccountAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EtsyAccountDto>> Update(int id, [FromBody] UpdateEtsyAccountDto dto, CancellationToken cancellationToken)
    {
        var updated = await _accountService.UpdateAccountAsync(id, dto, cancellationToken);
        if (updated == null) return NotFound(new { message = $"Mağaza bulunamadı (ID: {id})" });
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await _accountService.DeleteAccountAsync(id, cancellationToken);
        if (!success) return NotFound(new { message = $"Mağaza bulunamadı (ID: {id})" });
        return NoContent();
    }

    [HttpGet("{id:int}/oauth/authorize")]
    public async Task<ActionResult<OAuthAuthorizeResultDto>> InitiateOAuth(int id, [FromQuery] string? redirectUri, CancellationToken cancellationToken)
    {
        // Default redirect URI to self callback if not provided
        var callbackUrl = string.IsNullOrWhiteSpace(redirectUri)
            ? $"{Request.Scheme}://{Request.Host}/api/accounts/oauth/callback"
            : redirectUri;

        try
        {
            var result = await _accountService.InitiateOAuthAsync(id, callbackUrl, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("oauth/callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
    {
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/accounts/oauth/callback";

        var success = await _accountService.HandleOAuthCallbackAsync(state, code, redirectUri, cancellationToken);

        if (success)
        {
            // Redirect back to dashboard UI with success parameter
            return Redirect("/index.html?oauth=success");
        }

        return Redirect("/index.html?oauth=error");
    }

    [HttpPost("{id:int}/refresh-token")]
    public async Task<IActionResult> RefreshToken(int id, CancellationToken cancellationToken)
    {
        var success = await _accountService.RefreshAccountTokenIfNeededAsync(id, cancellationToken);
        if (success)
        {
            return Ok(new { message = "Token başarıyla yenilendi veya halen geçerli." });
        }
        return BadRequest(new { message = "Token yenilenemedi. Lütfen mağazayı tekrar bağlayın." });
    }
}
