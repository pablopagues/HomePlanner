using Application.HomePlanner.DTOs.Cardapio.Cardapio;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Domain.HomePlanner.Models.Enums;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class PlanejamentoSemanalRepository : IPlanejamentoSemanalRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public PlanejamentoSemanalRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<PlanejamentoSemanal?> ObterPorDataInicioAsync(DateOnly dataInicio, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.PlanejamentosSemanais
            .FirstOrDefaultAsync(p => p.DataInicio == dataInicio, ct);
    }

    public async Task<PlanejamentoSemanal?> ObterEntidadeAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.PlanejamentosSemanais
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<CardapioSemanaDTO?> ObterCardapioSemanaAsync(DateOnly dataInicio, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.PlanejamentosSemanais
            .Where(p => p.DataInicio == dataInicio)
            .Select(p => new CardapioSemanaDTO
            {
                Id        = p.Id,
                DataInicio = p.DataInicio,
                Nome      = p.Nome,
                Refeicoes = p.RefeicoesDia
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.DiaSemana).ThenBy(r => r.TipoRefeicao)
                    .Select(r => new RefeicaoDiaDTO
                    {
                        Id              = r.Id,
                        DiaSemana       = r.DiaSemana,
                        TipoRefeicao    = r.TipoRefeicao,
                        ReceitaId       = r.ReceitaId,
                        ReceitaNome     = r.Receita != null ? r.Receita.Nome : null,
                        ReceitaUrlImagem = r.Receita != null ? r.Receita.UrlImagem : null,
                        ReceitaTemFoto  = r.Receita != null && r.Receita.Foto != null,
                        ReceitaFotoAtualizadaEm = r.Receita != null ? r.Receita.FotoAtualizadaEm : null,
                        PorcoesDesejadas = r.PorcoesDesejadas,
                        Observacao      = r.Observacao,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<RefeicaoDia?> ObterRefeicaoDiaAsync(
        int planejamentoId, DiaSemana dia, TipoRefeicao tipo, CancellationToken ct = default)
    {
        // Usa _db para retornar entidade rastreada (writes)
        return await _db.RefeicoesDia
            .FirstOrDefaultAsync(r => r.PlanejamentoSemanalId == planejamentoId
                                   && r.DiaSemana == dia
                                   && r.TipoRefeicao == tipo
                                   && !r.IsDeleted, ct);
    }

    public async Task<RefeicaoDia?> ObterRefeicaoDiaPorIdAsync(int id, CancellationToken ct = default)
        => await _db.RefeicoesDia.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

    public async Task AdicionarAsync(PlanejamentoSemanal entidade, CancellationToken ct = default)
        => await _db.PlanejamentosSemanais.AddAsync(entidade, ct);

    public async Task AdicionarRefeicaoDiaAsync(RefeicaoDia entidade, CancellationToken ct = default)
        => await _db.RefeicoesDia.AddAsync(entidade, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
