namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Abre as páginas públicas (termos, privacidade, contato) no navegador do sistema.
///
/// O texto legal não é duplicado dentro do app de propósito: ele é versionado no
/// servidor (LgpdConstants.VersaoAtual) e mudaria sem o app acompanhar. O contato é
/// o canal oficial dos direitos do titular declarado na Política de Privacidade.
/// </summary>
public class LinksLegais
{
    private readonly SessaoAtual _sessao;

    public LinksLegais(SessaoAtual sessao) => _sessao = sessao;

    public string UrlTermos => Url("/termos");
    public string UrlPrivacidade => Url("/privacidade");
    public string UrlContato => Url("/contato");

    public Task AbrirTermosAsync() => AbrirAsync(UrlTermos);
    public Task AbrirPrivacidadeAsync() => AbrirAsync(UrlPrivacidade);
    public Task AbrirContatoAsync() => AbrirAsync(UrlContato);

    private string Url(string caminho) => $"{_sessao.BaseUrl.TrimEnd('/')}{caminho}";

    private static async Task AbrirAsync(string url)
    {
        try
        {
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            // Sem navegador disponível — não vale derrubar o app por isso.
        }
    }
}
