using Application.HomePlanner.DTOs.Onboarding;
using Application.HomePlanner.Services.Configuracao;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Configuração da família (tipos de refeição ativos, sistema de medidas, etc.).</summary>
public class ConfiguracaoController : ApiControllerBase
{
    private readonly IConfiguracaoFamiliaService _config;

    public ConfiguracaoController(IConfiguracaoFamiliaService config) => _config = config;

    /// <summary>Configuração atual da família.</summary>
    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct)
        => Ok(await _config.ObterAsync(ct));

    /// <summary>Cria/atualiza a configuração da família (não altera o estado do onboarding).</summary>
    [HttpPut]
    public async Task<IActionResult> Salvar([FromBody] ConfiguracaoFamiliaDTO dto, CancellationToken ct)
        => Responder(await _config.SalvarAsync(dto, ct));
}
