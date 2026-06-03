using Application.HomePlanner.Common;
using Application.HomePlanner.DTOs.Onboarding;

namespace Application.HomePlanner.Services.Configuracao;

public interface IConfiguracaoFamiliaService
{
    Task<ConfiguracaoFamiliaDTO> ObterAsync(CancellationToken ct = default);

    /// <summary>Cria/atualiza a configuração da família sem alterar o estado do onboarding.</summary>
    Task<ResultadoOperacao> SalvarAsync(ConfiguracaoFamiliaDTO dto, CancellationToken ct = default);
}
