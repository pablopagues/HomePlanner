using System.Net;

namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Injeta o Bearer token nas chamadas autenticadas e cuida do refresh:
/// - proativo: renova antes de enviar se o token está perto de expirar;
/// - reativo: em um 401, tenta renovar uma vez e reenvia (apenas GET, sem corpo).
/// </summary>
public class AuthMessageHandler : DelegatingHandler
{
    private readonly SessaoAtual _sessao;
    private readonly AuthApi _auth;
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AuthMessageHandler(SessaoAtual sessao, AuthApi auth)
    {
        _sessao = sessao;
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (_sessao.TokenQuaseExpirado)
            await RenovarComLockAsync(ct);

        AplicarToken(request);
        var resp = await base.SendAsync(request, ct);

        // Reativo: só reenvia requisições idempotentes sem corpo (evita rebobinar stream).
        if (resp.StatusCode == HttpStatusCode.Unauthorized && request.Method == HttpMethod.Get && request.Content is null)
        {
            if (await RenovarComLockAsync(ct))
            {
                resp.Dispose();
                using var novo = new HttpRequestMessage(HttpMethod.Get, request.RequestUri);
                AplicarToken(novo);
                return await base.SendAsync(novo, ct);
            }
        }

        return resp;
    }

    private void AplicarToken(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_sessao.AccessToken))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessao.AccessToken);
    }

    private async Task<bool> RenovarComLockAsync(CancellationToken ct)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            // Outro request pode já ter renovado enquanto esperávamos o lock.
            if (!_sessao.TokenQuaseExpirado && _sessao.EstaLogado) return true;
            return await _auth.RenovarAsync(ct);
        }
        finally { _refreshLock.Release(); }
    }
}
