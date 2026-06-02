using Application.HomePlanner.Common;
using Domain.HomePlanner.Models.Cardapio;

namespace Application.HomePlanner.Services.Cardapio;

public interface IConversorUnidadeService
{
    /// <summary>
    /// Converte quantidade entre unidades do mesmo tipo.
    /// Lança InvalidOperationException se os tipos forem incompatíveis.
    /// </summary>
    decimal Converter(decimal quantidade, UnidadeMedida origem, UnidadeMedida destino);

    bool SaoCompativeis(UnidadeMedida origem, UnidadeMedida destino);

    Task<ResultadoOperacao<decimal>> ConverterAsync(
        decimal quantidade, int unidadeOrigemId, int unidadeDestinoId, CancellationToken ct = default);
}
