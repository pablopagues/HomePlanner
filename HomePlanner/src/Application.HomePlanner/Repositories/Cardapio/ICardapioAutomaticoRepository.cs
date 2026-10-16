using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

/// <summary>Agendamento e rascunhos do cardápio automático (escopo do tenant atual, entidades rastreadas).</summary>
public interface ICardapioAutomaticoRepository
{
    Task<AgendamentoCardapioIA?> ObterAgendamentoAsync(CancellationToken ct = default);
    void AdicionarAgendamento(AgendamentoCardapioIA agendamento);

    Task<RascunhoCardapioIA?> ObterRascunhoAsync(DateOnly dataInicio, CancellationToken ct = default);
    Task<RascunhoCardapioIA?> ObterRascunhoPorIdAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// Insere e grava o rascunho na hora. false = já existe um para a mesma semana
    /// (outro ciclo/instância chegou antes — índice único).
    /// </summary>
    Task<bool> TentarCriarRascunhoAsync(RascunhoCardapioIA rascunho, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
