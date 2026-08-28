using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Resources.HomePlanner;

namespace HomePlanner.MobileApp.Services;

/// <summary>
/// Traduz o corpo de erro da API.
///
/// A API manda o <b>código</b> do erro; o texto sai daqui, no idioma do app. É o que
/// permite o servidor responder a qualquer cliente sem saber em que língua ele está.
/// Se o código for desconhecido (app mais antigo que a API), cai no texto que veio junto.
/// </summary>
public class ErroApi
{
    private readonly IStringLocalizer<SharedResource> _localizador;

    public ErroApi(IStringLocalizer<SharedResource> localizador) => _localizador = localizador;

    private class Corpo
    {
        public List<ItemErro>? Erros { get; set; }
        public List<string>? Mensagens { get; set; }
    }

    private class ItemErro
    {
        public string? Codigo { get; set; }
        public List<JsonElement>? Args { get; set; }
        public string? Texto { get; set; }
    }

    public async Task<string> LerAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var corpo = await resp.Content.ReadFromJsonAsync<Corpo>(cancellationToken: ct);

            if (corpo?.Erros is { Count: > 0 })
                return string.Join(" ", corpo.Erros.Select(Traduzir));

            // Endpoints que ainda respondem só com textos.
            if (corpo?.Mensagens is { Count: > 0 })
                return string.Join(" ", corpo.Mensagens);
        }
        catch { /* corpo não-JSON ou formato inesperado */ }

        return PorStatus((int)resp.StatusCode);
    }

    private string Traduzir(ItemErro erro)
    {
        // Sem código: veio do Identity/Stripe já em texto final.
        if (string.IsNullOrEmpty(erro.Codigo))
            return erro.Texto ?? PorStatus(0);

        var traduzido = _localizador[$"Erro_{erro.Codigo}"];
        if (traduzido.ResourceNotFound)
            return erro.Texto ?? erro.Codigo;

        if (erro.Args is not { Count: > 0 })
            return traduzido.Value;

        var args = erro.Args.Select(object (a) => a.ValueKind switch
        {
            JsonValueKind.Number => a.TryGetInt64(out var n) ? n : a.GetDouble(),
            JsonValueKind.True or JsonValueKind.False => a.GetBoolean(),
            _ => a.ToString(),
        }).ToArray();

        return string.Format(traduzido.Value, args);
    }

    private string PorStatus(int status) => status switch
    {
        401 => _localizador["Erro_sessao_expirada"],
        402 => _localizador["Assin_BloqTexto"],   // assinatura bloqueada
        403 => _localizador["Erro_sem_permissao"],
        404 => _localizador["Erro_nao_encontrado"],
        _ => $"{_localizador["App_Erro"]} {status}",
    };
}
