using Application.HomePlanner.DTOs.Cardapio.CardapioIA;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Monta o cardápio da semana via IA (Claude), escolhendo entre as receitas da casa.
/// Retorna <c>null</c> quando a IA está desabilitada ou falha. A resposta NÃO é
/// validada aqui — quem chama confere ids, horários e regras.
/// </summary>
public interface IGeradorCardapioIA
{
    bool Habilitado { get; }

    Task<RespostaCardapioIA?> GerarAsync(ContextoCardapioIA contexto, string idioma, CancellationToken ct = default);
}
