using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

public interface IImportadorReceitaService
{
    Task<ResultadoOperacao<ReceitaImportadaPreviewDTO>> ImportarDeUrlAsync(string url, CancellationToken ct = default);
}
