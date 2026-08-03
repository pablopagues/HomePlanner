namespace Application.HomePlanner.DTOs.Auth;

/// <summary>
/// Resposta do POST /api/auth/login. Quando o usuário tem 2FA ativo, vem
/// <see cref="Requer2FA"/> = true + <see cref="MfaToken"/> (curto), e os tokens
/// só são emitidos após POST /api/auth/2fa. Caso contrário, <see cref="Tokens"/> já vem preenchido.
/// </summary>
public class LoginRespostaDTO
{
    public bool Requer2FA { get; set; }

    /// <summary>Token curto (5 min) que prova que a senha foi validada. Enviar de volta no passo 2FA.</summary>
    public string? MfaToken { get; set; }

    /// <summary>Par de tokens — preenchido apenas quando o login não exige 2FA.</summary>
    public TokensDTO? Tokens { get; set; }
}
