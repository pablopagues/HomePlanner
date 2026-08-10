using System.Net.Http.Json;

namespace HomePlanner.MobileApp.Services;

/// <summary>Extrai a mensagem de erro do corpo padrão da API ({ "erros": [...] }).</summary>
public static class ErroApi
{
    private class ErroResp { public string[]? Erros { get; set; } }

    public static async Task<string> LerAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var corpo = await resp.Content.ReadFromJsonAsync<ErroResp>(cancellationToken: ct);
            if (corpo?.Erros is { Length: > 0 })
                return string.Join(", ", corpo.Erros);
        }
        catch { /* corpo não-JSON */ }

        return (int)resp.StatusCode switch
        {
            401 => "Sessão expirada. Entre novamente.",
            403 => "Você não tem permissão para esta ação.",
            404 => "Não encontrado.",
            _ => $"Erro {(int)resp.StatusCode}.",
        };
    }
}
