using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.ListaCompras;

namespace Application.HomePlanner.Services.ListaCompras;

public interface IListaComprasService
{
    /// <summary>
    /// Calcula a lista de compras agregada de todas as receitas da semana,
    /// somando quantidades do mesmo ingrediente e convertendo para a melhor unidade.
    /// </summary>
    Task<ResultadoOperacao<ListaComprasDTO>> CalcularDaSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default);

    /// <summary>Mapa IngredienteId → "já tenho/comprado" compartilhado da semana.</summary>
    Task<IReadOnlyDictionary<int, bool>> ObterMarcacoesSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default);

    /// <summary>Marca/desmarca um item (ingrediente) da semana — estado compartilhado.</summary>
    Task<ResultadoOperacao> MarcarItemAsync(
        DateOnly dataInicio, int ingredienteId, bool comprado, CancellationToken ct = default);

    /// <summary>Desmarca todos os itens da semana.</summary>
    Task<ResultadoOperacao> LimparMarcacoesSemanaAsync(
        DateOnly dataInicio, CancellationToken ct = default);
}
