using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Auth;

namespace Application.HomePlanner.Services.Auth;

/// <summary>
/// Emissão e renovação de tokens JWT para a API (apps mobile).
/// Reaproveita o Identity/SignInManager e a fábrica de claims existente,
/// garantindo que o app veja exatamente os mesmos claims do login por cookie.
/// </summary>
public interface IAuthTokenService
{
    /// <summary>
    /// Valida e-mail/senha. Se o usuário tem 2FA, devolve <see cref="LoginRespostaDTO.Requer2FA"/> = true
    /// com um MfaToken curto; senão, já devolve os tokens.
    /// </summary>
    Task<ResultadoOperacao<LoginRespostaDTO>> LoginAsync(LoginApiDTO dto, CancellationToken ct = default);

    /// <summary>Segundo passo do 2FA: valida o MfaToken + código e emite o par de tokens.</summary>
    Task<ResultadoOperacao<TokensDTO>> Confirmar2FAAsync(Confirmar2FADTO dto, CancellationToken ct = default);

    /// <summary>Rotaciona um refresh token válido, devolvendo um novo par.</summary>
    Task<ResultadoOperacao<TokensDTO>> RenovarAsync(RefreshApiDTO dto, CancellationToken ct = default);

    /// <summary>Revoga um refresh token (logout do dispositivo).</summary>
    Task<ResultadoOperacao> RevogarAsync(string refreshToken, CancellationToken ct = default);
}
