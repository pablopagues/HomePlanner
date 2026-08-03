using Application.HomePlanner.DTOs.Onboarding;
using Application.HomePlanner.Services.Onboarding;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Onboarding inicial da família (disponível tanto no app quanto na web).</summary>
public class OnboardingController : ApiControllerBase
{
    private readonly IOnboardingService _onboarding;

    public OnboardingController(IOnboardingService onboarding) => _onboarding = onboarding;

    /// <summary>Configuração atual (pré-preenche o formulário de onboarding).</summary>
    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct)
        => Ok(await _onboarding.ObterConfiguracaoAtualAsync(ct));

    /// <summary>Finaliza o onboarding, gravando a configuração e marcando-o como concluído.</summary>
    [HttpPost("finalizar")]
    public async Task<IActionResult> Finalizar([FromBody] ConfiguracaoFamiliaDTO dto, CancellationToken ct)
        => Responder(await _onboarding.FinalizarAsync(dto, ct));
}
