using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Cardapio.Receita;

namespace Application.HomePlanner.Services.Cardapio;

/// <summary>
/// Controla a cota mensal de importação de receitas por plano (contada ao salvar)
/// e o throttle anti-abuso das chamadas à IA (toda chamada, URL ou texto manual).
/// </summary>
public interface ICotaImportacaoService
{
    /// <summary>Pré-check (read-only): o tenant ainda pode importar receitas este mês?</summary>
    Task<ResultadoOperacao> VerificarPodeImportarAsync(CancellationToken ct = default);

    /// <summary>Throttle por tenant/hora para qualquer chamada à IA (não é cota de plano).</summary>
    Task<ResultadoOperacao> VerificarThrottleIAAsync(CancellationToken ct = default);

    /// <summary>Incrementa o contador do mês ao salvar uma receita importada.</summary>
    Task RegistrarImportacaoAsync(CancellationToken ct = default);

    /// <summary>Uso atual (usado/limite) para exibição na UI.</summary>
    Task<UsoCotaImportacaoDTO> ObterUsoAsync(CancellationToken ct = default);
}
