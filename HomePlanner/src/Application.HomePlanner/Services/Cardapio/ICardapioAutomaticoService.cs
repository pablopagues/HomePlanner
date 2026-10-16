using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.CardapioIA;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Rascunho automático do cardápio por IA: a família agenda um dia/hora e a IA prepara
/// a semana seguinte sozinha (serviço de fundo), deixando um rascunho para revisar.
/// </summary>
public interface ICardapioAutomaticoService
{
    /// <summary>Agendamento salvo (também guarda as regras padrão do diálogo), ou null.</summary>
    Task<AgendamentoCardapioIADTO?> ObterAgendamentoAsync(CancellationToken ct = default);

    /// <summary>Liga/desliga e grava dia, hora e regras. Só adultos.</summary>
    Task<ResultadoOperacao> SalvarAgendamentoAsync(AgendamentoCardapioIADTO dto, CancellationToken ct = default);

    /// <summary>Rascunho pronto (aguardando revisão) da semana, para adultos; senão null.</summary>
    Task<RascunhoCardapioIADTO?> ObterRascunhoProntoAsync(DateOnly dataInicio, CancellationToken ct = default);

    Task<ResultadoOperacao> DescartarRascunhoAsync(int id, CancellationToken ct = default);

    /// <summary>Grava a prévia no cardápio e marca o rascunho da semana (se houver) como aplicado.</summary>
    Task<ResultadoOperacao> AplicarAsync(CardapioSugeridoDTO sugestao, CancellationToken ct = default);

    /// <summary>
    /// Usado pelo serviço de fundo, com o contexto do tenant já definido (como o Owner):
    /// gera o rascunho da semana <paramref name="dataInicio"/> se ainda não existir.
    /// </summary>
    Task<ResultadoRascunhoAutomatico> GerarRascunhoAsync(DateOnly dataInicio, string idioma, CancellationToken ct = default);
}
