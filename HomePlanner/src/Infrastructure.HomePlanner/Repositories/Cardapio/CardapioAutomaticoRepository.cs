using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class CardapioAutomaticoRepository : ICardapioAutomaticoRepository
{
    private readonly HomePlannerDbContext _db;

    public CardapioAutomaticoRepository(HomePlannerDbContext db) => _db = db;

    public Task<AgendamentoCardapioIA?> ObterAgendamentoAsync(CancellationToken ct = default)
        => _db.AgendamentosCardapioIA.FirstOrDefaultAsync(ct);

    public void AdicionarAgendamento(AgendamentoCardapioIA agendamento)
        => _db.AgendamentosCardapioIA.Add(agendamento);

    public Task<RascunhoCardapioIA?> ObterRascunhoAsync(DateOnly dataInicio, CancellationToken ct = default)
        => _db.RascunhosCardapioIA.FirstOrDefaultAsync(r => r.DataInicio == dataInicio, ct);

    public Task<RascunhoCardapioIA?> ObterRascunhoPorIdAsync(int id, CancellationToken ct = default)
        => _db.RascunhosCardapioIA.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<bool> TentarCriarRascunhoAsync(RascunhoCardapioIA rascunho, CancellationToken ct = default)
    {
        var entry = _db.RascunhosCardapioIA.Add(rascunho);
        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            entry.State = EntityState.Detached; // não tenta inserir de novo no próximo SaveChanges
            return false;
        }
    }

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
