namespace Application.HomePlanner.DTOs.Familia;

public class ConviteMembroResultadoDTO
{
    public string UsuarioId { get; init; } = string.Empty;

    /// <summary>True quando o convite foi enviado por e-mail.</summary>
    public bool EmailEnviado { get; init; }

    /// <summary>Link de definição de senha, retornado quando o SMTP está desabilitado (dev),
    /// para o Owner compartilhar manualmente. Nulo quando enviado por e-mail.</summary>
    public string? LinkConvite { get; init; }
}
