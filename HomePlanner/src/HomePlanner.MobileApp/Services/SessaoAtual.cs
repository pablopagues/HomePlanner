using Microsoft.Maui.Storage;

namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Estado da sessão do app: URL base da API e tokens. O access token fica em memória;
/// o refresh token e a URL base são persistidos (SecureStorage/Preferences) entre execuções.
/// </summary>
public class SessaoAtual
{
    private const string ChaveRefresh = "hp_refresh_token";
    private const string ChaveBaseUrl = "hp_base_url";

    /// <summary>URL padrão da API (produção). Pode ser trocada na tela de login (ex.: localhost em dev).</summary>
    public const string BaseUrlPadrao = "https://homeplanner.siderisx.ca";

    /// <summary>Disparado quando o estado de autenticação muda (login/logout).</summary>
    public event Action? Alterou;

    public string BaseUrl { get; set; } = Preferences.Default.Get(ChaveBaseUrl, BaseUrlPadrao);

    public string? AccessToken { get; private set; }
    public DateTime AccessTokenExpiraEm { get; private set; }
    public string? RefreshToken { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public bool EhOwner { get; private set; }

    /// <summary>MfaToken pendente entre o passo de senha e a tela de 2FA.</summary>
    public string? MfaTokenPendente { get; set; }

    public bool EstaLogado => !string.IsNullOrEmpty(AccessToken);

    /// <summary>Access token perto de expirar (dá margem de 30s para renovar proativamente).</summary>
    public bool TokenQuaseExpirado => EstaLogado && DateTime.UtcNow >= AccessTokenExpiraEm.AddSeconds(-30);

    public void SalvarBaseUrl(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        Preferences.Default.Set(ChaveBaseUrl, BaseUrl);
    }

    public async Task DefinirTokensAsync(TokensDTO tokens)
    {
        AccessToken = tokens.AccessToken;
        AccessTokenExpiraEm = tokens.AccessTokenExpiraEm;
        RefreshToken = tokens.RefreshToken;
        NomeCompleto = tokens.NomeCompleto;
        EhOwner = tokens.EhOwner;
        await SecureStorage.Default.SetAsync(ChaveRefresh, tokens.RefreshToken);
        Alterou?.Invoke();
    }

    /// <summary>Recupera o refresh token persistido (para tentar auto-login ao abrir o app).</summary>
    public async Task<string?> CarregarRefreshPersistidoAsync()
        => RefreshToken ??= await SecureStorage.Default.GetAsync(ChaveRefresh);

    public void Sair()
    {
        AccessToken = null;
        RefreshToken = null;
        NomeCompleto = string.Empty;
        EhOwner = false;
        SecureStorage.Default.Remove(ChaveRefresh);
        Alterou?.Invoke();
    }
}
