using Application.HomePlanner.DTOs.ListaCompras;
using Application.HomePlanner.DTOs.Planner;
using Application.HomePlanner.Repositories.Cardapio;
using Domain.HomePlanner.Models.Cardapio;
using Infrastructure.HomePlanner.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.HomePlanner.Repositories.Cardapio;

public class PedidoCompraRepository : IPedidoCompraRepository
{
    private readonly HomePlannerDbContext _db;
    private readonly IDbContextFactory<HomePlannerDbContext> _dbFactory;

    public PedidoCompraRepository(HomePlannerDbContext db, IDbContextFactory<HomePlannerDbContext> dbFactory)
    {
        _db = db;
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<PedidoCompraDTO>> ListarDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        // Filtro global de tenant/soft-delete já aplicado.
        return await db.PedidosCompra
            .Where(p => p.DataInicioSemana == dataInicio)
            .OrderBy(p => p.Solicitante != null ? p.Solicitante.NomeCompleto : string.Empty)
            .ThenBy(p => p.Comprado)
            .ThenBy(p => p.Descricao)
            .Select(p => new PedidoCompraDTO
            {
                Id                          = p.Id,
                Descricao                   = p.Descricao,
                Quantidade                  = p.Quantidade,
                SolicitanteUsuarioId        = p.SolicitanteUsuarioId,
                SolicitanteNome             = p.Solicitante != null ? p.Solicitante.NomeCompleto : string.Empty,
                SolicitanteFotoAtualizadaEm = p.Solicitante != null ? p.Solicitante.FotoAtualizadaEm : null,
                Comprado                    = p.Comprado,
                ListaId                     = p.ListaId,
            })
            .ToListAsync(ct);
    }

    public async Task<PedidoCompra?> ObterEntidadeAsync(int id, CancellationToken ct = default)
        => await _db.PedidosCompra.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<MembroFamiliaDTO>> ListarMembrosFamiliaAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Users
            .Where(u => u.Ativo)
            .OrderBy(u => u.NomeCompleto)
            .Select(u => new MembroFamiliaDTO
            {
                UsuarioId        = u.Id,
                Nome             = u.NomeCompleto,
                FotoAtualizadaEm = u.FotoAtualizadaEm,
            })
            .ToListAsync(ct);
    }

    public async Task AdicionarAsync(PedidoCompra entidade, CancellationToken ct = default)
        => await _db.PedidosCompra.AddAsync(entidade, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
