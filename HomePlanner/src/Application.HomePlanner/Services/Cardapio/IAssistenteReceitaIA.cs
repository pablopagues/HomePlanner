using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Assistente de receitas via IA (Claude): extrai uma receita de um texto livre colado
/// (site, ChatGPT, mensagem) e sugere receitas novas a partir de um pedido.
/// Os métodos retornam <c>null</c> quando a IA está desabilitada ou falha.
/// </summary>
public interface IAssistenteReceitaIA
{
    bool Habilitado { get; }

    /// <summary>Tamanho máximo aceito para o texto colado em <see cref="ExtrairDeTextoAsync"/>.</summary>
    int MaxCaracteresTexto { get; }

    /// <summary>
    /// Extrai nome, porções, tempo, linhas de ingredientes e modo de preparo do texto,
    /// sem traduzir nem inventar. Retorna um preview com <c>Sucesso=false</c> quando o
    /// texto não contém uma receita.
    /// </summary>
    Task<ReceitaImportadaPreviewDTO?> ExtrairDeTextoAsync(string texto, CancellationToken ct = default);

    /// <summary>
    /// Gera 3 sugestões completas de receita para o pedido, no idioma informado.
    /// </summary>
    /// <param name="receitasDaCasa">Nomes das receitas da família (gosto + não repetir).</param>
    Task<IReadOnlyList<ReceitaImportadaPreviewDTO>?> SugerirAsync(
        PedidoSugestaoReceitaDTO pedido,
        IReadOnlyList<string> receitasDaCasa,
        string idioma,
        CancellationToken ct = default);
}
