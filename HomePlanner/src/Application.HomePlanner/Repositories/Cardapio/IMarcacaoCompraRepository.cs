using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Repositories.Cardapio;

public interface IMarcacaoCompraRepository
{
    /// <summary>Mapa IngredienteId → Comprado das marcações da semana (do tenant atual).</summary>
    Task<IReadOnlyDictionary<int, bool>> ObterDaSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);

    /// <summary>Marcação rastreada de um ingrediente na semana (para upsert), ou null.</summary>
    Task<MarcacaoCompra?> ObterAsync(DateOnly dataInicio, int ingredienteId, CancellationToken ct = default);

    /// <summary>Marcações rastreadas da semana (para limpar em lote).</summary>
    Task<IReadOnlyList<MarcacaoCompra>> ListarRastreadasDaSemanaAsync(DateOnly dataInicio, CancellationToken ct = default);

    Task AdicionarAsync(MarcacaoCompra entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}
