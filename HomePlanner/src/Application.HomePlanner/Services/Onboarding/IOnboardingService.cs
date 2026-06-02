using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Onboarding;

namespace Application.HomePlanner.Services.Onboarding;

public interface IOnboardingService
{
    Task<ConfiguracaoFamiliaDTO> ObterConfiguracaoAtualAsync(CancellationToken ct = default);
    Task<ResultadoOperacao> FinalizarAsync(ConfiguracaoFamiliaDTO dto, CancellationToken ct = default);
}
