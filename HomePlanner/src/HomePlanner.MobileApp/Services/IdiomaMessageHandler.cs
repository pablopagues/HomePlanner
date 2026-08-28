using System.Net.Http.Headers;

namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Põe o Accept-Language em toda chamada à API. Fica num handler, e não em cada método,
/// porque esquecer um endpoint faria só aquela mensagem de erro voltar em português —
/// o tipo de falha que ninguém nota até um usuário reclamar.
///
/// Do lado do servidor, quem lê esse header em /api é o AcceptLanguageHeaderRequestCultureProvider.
/// </summary>
public class IdiomaMessageHandler : DelegatingHandler
{
    private readonly IdiomaService _idioma;

    public IdiomaMessageHandler(IdiomaService idioma) => _idioma = idioma;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(IdiomaService.ParaCultura(_idioma.Atual).Name));

        return base.SendAsync(request, ct);
    }
}
