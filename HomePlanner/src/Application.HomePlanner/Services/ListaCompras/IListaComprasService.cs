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
}
