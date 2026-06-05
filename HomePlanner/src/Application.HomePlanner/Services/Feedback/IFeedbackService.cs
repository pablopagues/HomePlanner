using Application.HomePlanner.DTOs.Feedback;

namespace Application.HomePlanner.Services.Feedback;

/// <summary>
/// Recebe feedback do usuário (bug, sugestão, elogio, outro) e envia para a equipe.
/// </summary>
public interface IFeedbackService
{
    Task<FeedbackEnvioResultado> EnviarAsync(FeedbackDTO dto, CancellationToken ct = default);
}

public class FeedbackEnvioResultado
{
    public bool Sucesso { get; set; }
    public string? Erro { get; set; }

    public static FeedbackEnvioResultado Ok() => new() { Sucesso = true };
    public static FeedbackEnvioResultado Falha(string erro) => new() { Sucesso = false, Erro = erro };
}
