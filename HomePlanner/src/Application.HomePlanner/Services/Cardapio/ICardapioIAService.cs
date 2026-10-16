using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.CardapioIA;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Cardápio da semana gerado por IA a partir das receitas da casa. As regras rígidas
/// (não repetir, horários, tempo máximo) são aplicadas em código antes e depois da IA;
/// a IA só cuida das preferências. Gerar não grava nada: a prévia só entra no cardápio
/// em <see cref="AplicarAsync"/>.
/// </summary>
public interface ICardapioIAService
{
    /// <summary>Adulto da família e IA configurada.</summary>
    Task<bool> PodeGerarAsync();

    /// <summary>Contagem de horários e receitas disponíveis (sem IA e sem gastar cota).</summary>
    Task<ResultadoOperacao<PrevisaoCardapioIADTO>> PreverAsync(PedidoCardapioIADTO pedido, CancellationToken ct = default);

    /// <summary>Gera a prévia; consome 1 da cota mensal quando dá certo.</summary>
    Task<ResultadoOperacao<CardapioSugeridoDTO>> GerarAsync(
        PedidoCardapioIADTO pedido, string idioma, CancellationToken ct = default);

    /// <summary>Grava no cardápio os horários da prévia que têm prato (os vazios ficam como estão).</summary>
    Task<ResultadoOperacao> AplicarAsync(CardapioSugeridoDTO sugestao, CancellationToken ct = default);
}
