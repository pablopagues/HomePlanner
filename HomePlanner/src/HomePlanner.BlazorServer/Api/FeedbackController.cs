using Application.HomePlanner.DTOs.Feedback;
using Application.HomePlanner.Services.Feedback;
using Microsoft.AspNetCore.Mvc;

namespace HomePlanner.BlazorServer.Api;

/// <summary>Envio de feedback in-app (bug, sugestão, elogio, outro).</summary>
public class FeedbackController : ApiControllerBase
{
    private readonly IFeedbackService _feedback;

    public FeedbackController(IFeedbackService feedback) => _feedback = feedback;

    /// <summary>Envia um feedback para a equipe.</summary>
    [HttpPost]
    public async Task<IActionResult> Enviar([FromBody] FeedbackDTO dto, CancellationToken ct)
    {
        var r = await _feedback.EnviarAsync(dto, ct);
        return r.Sucesso ? NoContent() : BadRequest(new { erros = new[] { r.Erro ?? "Falha ao enviar feedback." } });
    }
}
