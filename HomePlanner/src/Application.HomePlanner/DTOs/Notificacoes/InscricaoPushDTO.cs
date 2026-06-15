namespace Application.HomePlanner.DTOs.Notificacoes;

/// <summary>Dados da assinatura de push vindos do navegador (via JS interop).</summary>
public class InscricaoPushDTO
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
}
