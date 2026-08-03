using Application.HomePlanner.DTOs.Empresa;
using Application.HomePlanner.Services.Empresa;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Dados da conta/tenant e confirmação de e-mail do usuário atual.</summary>
public class EmpresaController : ApiControllerBase
{
    private readonly IEmpresaService _empresa;

    public EmpresaController(IEmpresaService empresa) => _empresa = empresa;

    /// <summary>Dados do tenant + e-mail/status de confirmação.</summary>
    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct)
        => Ok(await _empresa.ObterAsync(ct));

    /// <summary>Atualiza os dados editáveis do tenant.</summary>
    [HttpPut]
    public async Task<IActionResult> Salvar([FromBody] AtualizarEmpresaDTO dto, CancellationToken ct)
    {
        var erros = await _empresa.SalvarDadosAsync(dto, ct);
        return erros.Count == 0 ? NoContent() : BadRequest(new { erros });
    }

    /// <summary>Reenvia o e-mail de confirmação para o usuário atual.</summary>
    [HttpPost("reenviar-confirmacao")]
    public async Task<IActionResult> ReenviarConfirmacao(CancellationToken ct)
        => Responder(await _empresa.ReenviarConfirmacaoEmailAsync($"{Request.Scheme}://{Request.Host}", ct));
}
