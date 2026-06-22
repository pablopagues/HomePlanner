using Application.HomePlanner.DTOs.Cardapio.UnidadeMedida;
using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.Helpers;

/// <summary>
/// Conversão de exibição entre sistema métrico e imperial, dentro da mesma
/// dimensão (peso↔peso, volume↔volume). Opera sobre a lista de unidades já
/// carregada na UI. Não converte volume↔peso (exige densidade) nem unidades
/// contáveis (un, dente, fatia, pacote).
/// </summary>
public static class ConversorSistemaMedida
{
    private static readonly HashSet<string> _imperiais =
        new(StringComparer.OrdinalIgnoreCase) { "oz", "lb", "floz", "cup", "pint", "quart" };

    /// <summary>Sistema a que o código de unidade pertence.</summary>
    public static SistemaMedida SistemaDe(string? codigo)
        => codigo is not null && _imperiais.Contains(codigo) ? SistemaMedida.Imperial : SistemaMedida.Metrico;

    // Unidades-alvo canônicas por (sistema, tipo). Culinárias (xícara, colher)
    // não são alvo de conversão — só ml/l, g/kg, oz/lb, floz/cup/pint/quart.
    private static string[] CodigosAlvo(SistemaMedida sis, TipoUnidadeMedida tipo) => (sis, tipo) switch
    {
        (SistemaMedida.Metrico,  TipoUnidadeMedida.Massa)  => ["g", "kg"],
        (SistemaMedida.Metrico,  TipoUnidadeMedida.Volume) => ["ml", "l"],
        (SistemaMedida.Imperial, TipoUnidadeMedida.Massa)  => ["oz", "lb"],
        (SistemaMedida.Imperial, TipoUnidadeMedida.Volume) => ["floz", "cup", "pint", "quart"],
        _                                                  => [],
    };

    /// <summary>
    /// Converte (quantidade, unidade) para o <paramref name="alvo"/>, escolhendo a
    /// unidade canônica de melhor magnitude. Retorna o original quando não há conversão
    /// aplicável (tipo contável, sem unidade, ou já no sistema alvo sem alvo canônico).
    /// </summary>
    public static (decimal Quantidade, int UnidadeMedidaId) Converter(
        decimal quantidade, int unidadeMedidaId, SistemaMedida alvo,
        IReadOnlyList<UnidadeMedidaListaDTO> unidades)
    {
        var origem = unidades.FirstOrDefault(u => u.Id == unidadeMedidaId);
        if (origem is null || origem.Tipo == TipoUnidadeMedida.Unidade || origem.FatorParaBase <= 0)
            return (quantidade, unidadeMedidaId);

        var alvoCodigos = CodigosAlvo(alvo, origem.Tipo);
        if (alvoCodigos.Length == 0) return (quantidade, unidadeMedidaId);

        var candidatos = unidades
            .Where(u => alvoCodigos.Contains(u.Codigo, StringComparer.OrdinalIgnoreCase) && u.FatorParaBase > 0)
            .OrderBy(u => u.FatorParaBase)
            .ToList();
        if (candidatos.Count == 0) return (quantidade, unidadeMedidaId);

        // Já está numa unidade-alvo do sistema desejado: não mexe.
        if (candidatos.Any(c => c.Id == origem.Id)) return (quantidade, unidadeMedidaId);

        var emBase = quantidade * origem.FatorParaBase;
        // Maior unidade cujo valor convertido seja >= 1; senão a menor.
        var escolhida = candidatos.LastOrDefault(u => emBase / u.FatorParaBase >= 1m) ?? candidatos[0];

        var novaQtd = Math.Round(emBase / escolhida.FatorParaBase, 2, MidpointRounding.AwayFromZero);
        return (novaQtd, escolhida.Id);
    }
}
