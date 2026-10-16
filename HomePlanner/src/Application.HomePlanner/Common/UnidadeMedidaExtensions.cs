using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Application.HomePlanner.Common;

/// <summary>
/// Exibição das unidades de medida no idioma atual. O banco guarda o código interno
/// ("xic", "cs", "un"...) e o nome em português; a tela traduz pelo código
/// (chaves <c>Un_{codigo}</c> = abreviação, <c>UnNome_{codigo}</c> = nome).
/// </summary>
public static class UnidadeMedidaExtensions
{
    /// <summary>Abreviação ("xíc.", "cup", "tbsp"...). Sem chave, devolve o próprio código.</summary>
    public static string AbrevUnidade(this IStringLocalizer localizador, string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return string.Empty;
        var t = localizador[$"Un_{codigo}"];
        return t.ResourceNotFound ? codigo : t.Value;
    }

    /// <summary>Nome por extenso. Sem chave, devolve o nome gravado no banco.</summary>
    public static string NomeUnidade(this IStringLocalizer localizador, string? codigo, string nomePadrao)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return nomePadrao;
        var t = localizador[$"UnNome_{codigo}"];
        return t.ResourceNotFound ? nomePadrao : t.Value;
    }

    /// <summary>"1,5 xíc." / "1.5 cup" — número no formato da cultura atual + unidade traduzida.</summary>
    public static string FormatarQuantidade(this IStringLocalizer localizador, decimal quantidade, string? codigo)
    {
        var numero = quantidade == Math.Truncate(quantidade)
            ? quantidade.ToString("0", CultureInfo.CurrentCulture)
            : quantidade.ToString("0.##", CultureInfo.CurrentCulture);
        var unidade = localizador.AbrevUnidade(codigo);
        return unidade.Length == 0 ? numero : $"{numero} {unidade}";
    }
}
