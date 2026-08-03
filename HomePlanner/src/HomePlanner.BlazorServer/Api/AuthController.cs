using Application.HomePlanner.DTOs.Auth;
using Application.HomePlanner.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Login e renovação de tokens para os apps mobile.</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthTokenService _auth;

    public AuthController(IAuthTokenService auth) => _auth = auth;

    /// <summary>
    /// Valida credenciais. Sem 2FA: devolve os tokens. Com 2FA: devolve requer2FA + mfaToken
    /// (chame então POST /api/auth/2fa).
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginApiDTO dto, CancellationToken ct)
    {
        var r = await _auth.LoginAsync(dto, ct);
        return r.Sucesso ? Ok(r.Valor) : Unauthorized(new { erros = r.Erros });
    }

    /// <summary>Segundo passo do login com 2FA: valida o código e devolve o par de tokens.</summary>
    [HttpPost("2fa")]
    public async Task<IActionResult> Confirmar2FA([FromBody] Confirmar2FADTO dto, CancellationToken ct)
    {
        var r = await _auth.Confirmar2FAAsync(dto, ct);
        return r.Sucesso ? Ok(r.Valor) : Unauthorized(new { erros = r.Erros });
    }

    /// <summary>Troca um refresh token válido por um novo par de tokens (rotação).</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshApiDTO dto, CancellationToken ct)
    {
        var r = await _auth.RenovarAsync(dto, ct);
        return r.Sucesso ? Ok(r.Valor) : Unauthorized(new { erros = r.Erros });
    }

    /// <summary>Revoga um refresh token (logout do dispositivo).</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshApiDTO dto, CancellationToken ct)
    {
        await _auth.RevogarAsync(dto.RefreshToken, ct);
        return NoContent();
    }
}
