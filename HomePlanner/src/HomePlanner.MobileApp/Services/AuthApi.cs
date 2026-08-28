using System.Net.Http.Json;
using Microsoft.Maui.Devices;

namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Chamadas de autenticação (NÃO passam pelo AuthMessageHandler — evita recursão no refresh).
/// </summary>
public class AuthApi
{
    private readonly HttpClient _http;
    private readonly SessaoAtual _sessao;
    private readonly ErroApi _erros;

    public AuthApi(HttpClient http, SessaoAtual sessao, ErroApi erros)
    {
        _http = http;
        _sessao = sessao;
        _erros = erros;
    }

    private string Url(string caminho) => $"{_sessao.BaseUrl.TrimEnd('/')}{caminho}";
    private static string Dispositivo => DeviceInfo.Current.Model;

    public async Task<(LoginRespostaDTO? resposta, string? erro)> LoginAsync(string email, string senha, CancellationToken ct = default)
    {
        try
        {
            var body = new LoginRequest { Email = email, Senha = senha, DispositivoInfo = Dispositivo };
            var resp = await _http.PostAsJsonAsync(Url("/api/auth/login"), body, ct);
            if (resp.IsSuccessStatusCode)
            {
                var dto = await resp.Content.ReadFromJsonAsync<LoginRespostaDTO>(cancellationToken: ct);
                if (dto?.Tokens is not null) await _sessao.DefinirTokensAsync(dto.Tokens);
                return (dto, null);
            }
            return (null, await _erros.LerAsync(resp, ct));
        }
        catch (Exception ex) { return (null, $"Falha de conexão: {ex.Message}"); }
    }

    public async Task<string?> Confirmar2FAAsync(string mfaToken, string codigo, bool codigoRecuperacao, CancellationToken ct = default)
    {
        try
        {
            var body = new Confirmar2FARequest
            {
                MfaToken = mfaToken, Codigo = codigo, CodigoRecuperacao = codigoRecuperacao, DispositivoInfo = Dispositivo,
            };
            var resp = await _http.PostAsJsonAsync(Url("/api/auth/2fa"), body, ct);
            if (resp.IsSuccessStatusCode)
            {
                var tokens = await resp.Content.ReadFromJsonAsync<TokensDTO>(cancellationToken: ct);
                if (tokens is not null) { await _sessao.DefinirTokensAsync(tokens); return null; }
                return "Resposta inválida do servidor.";
            }
            return await _erros.LerAsync(resp, ct);
        }
        catch (Exception ex) { return $"Falha de conexão: {ex.Message}"; }
    }

    /// <summary>Renova os tokens usando o refresh persistido. Devolve true se conseguiu.</summary>
    public async Task<bool> RenovarAsync(CancellationToken ct = default)
    {
        var refresh = await _sessao.CarregarRefreshPersistidoAsync();
        if (string.IsNullOrEmpty(refresh)) return false;

        try
        {
            var body = new RefreshRequest { RefreshToken = refresh, DispositivoInfo = Dispositivo };
            var resp = await _http.PostAsJsonAsync(Url("/api/auth/refresh"), body, ct);
            if (!resp.IsSuccessStatusCode) return false;

            var tokens = await resp.Content.ReadFromJsonAsync<TokensDTO>(cancellationToken: ct);
            if (tokens is null) return false;
            await _sessao.DefinirTokensAsync(tokens);
            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Pede o e-mail de redefinição. A API responde 204 exista o e-mail ou não, então
    /// a tela mostra sempre a mesma mensagem — não é falta de tratamento, é de propósito.
    /// </summary>
    public async Task<string?> EsqueciSenhaAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(Url("/api/auth/esqueci-senha"), new { Email = email }, ct);
            return resp.IsSuccessStatusCode ? null : await _erros.LerAsync(resp, ct);
        }
        catch (Exception ex) { return $"Falha de conexão: {ex.Message}"; }
    }

    public async Task<string?> RegistrarAsync(RegistroRequest req, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(Url("/api/registro"), req, ct);
            return resp.IsSuccessStatusCode ? null : await _erros.LerAsync(resp, ct);
        }
        catch (Exception ex) { return $"Falha de conexão: {ex.Message}"; }
    }
}
