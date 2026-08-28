namespace Application.HomePlanner.DTOs.Auth;

/// <summary>Par de tokens devolvido no login e no refresh.</summary>
public class TokensDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Instante (UTC) em que o access token expira.</summary>
    public DateTime AccessTokenExpiraEm { get; set; }

    public string TokenType { get; set; } = "Bearer";

    // Dados básicos do usuário para o app montar a UI sem outra chamada.
    public string UsuarioId { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public bool EhOwner { get; set; }

    /// <summary>
    /// Falso quando a família ainda não concluiu o onboarding — o app deve desviar para
    /// a tela de onboarding. Na web quem faz isso é o OnboardingRequiredMiddleware, mas
    /// ele ignora /api de propósito, então no app o desvio é responsabilidade do cliente.
    /// </summary>
    public bool OnboardingCompleto { get; set; }
}
