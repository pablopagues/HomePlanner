namespace Domain.HomePlanner.Models.SaaS.Options;

/// <summary>
/// Configuração da autenticação JWT usada pela API (apps mobile).
/// A chave (SecretKey) deve ficar em user-secrets/variável de ambiente no servidor,
/// nunca comitada. Precisa ter no mínimo 32 bytes para HMAC-SHA256.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Liga/desliga a emissão de tokens da API. Sem chave, fica desabilitado.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Chave simétrica de assinatura (HMAC-SHA256). Guardar em user-secrets/ambiente.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Emissor esperado no token (iss).</summary>
    public string Issuer { get; set; } = "HomePlanner";

    /// <summary>Audiência esperada no token (aud).</summary>
    public string Audience { get; set; } = "HomePlanner.App";

    /// <summary>Validade do access token, em minutos.</summary>
    public int AccessTokenMinutos { get; set; } = 60;

    /// <summary>Validade do refresh token, em dias.</summary>
    public int RefreshTokenDias { get; set; } = 60;

    /// <summary>Só utilizável quando ligado E com chave suficientemente longa.</summary>
    public bool EstaConfigurado =>
        IsEnabled && !string.IsNullOrWhiteSpace(SecretKey) && SecretKey.Length >= 32;
}
