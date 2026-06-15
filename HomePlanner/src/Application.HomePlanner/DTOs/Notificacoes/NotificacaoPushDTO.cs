namespace Application.HomePlanner.DTOs.Notificacoes;

/// <summary>Conteúdo de uma notificação a ser entregue. Serializado como payload do push.</summary>
public class NotificacaoPushDTO
{
    public string Titulo { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;

    /// <summary>Caminho aberto ao clicar na notificação. Default "/".</summary>
    public string? Url { get; set; }

    /// <summary>Ícone exibido (caminho). Default "/favicon.png".</summary>
    public string? Icone { get; set; }

    /// <summary>Agrupa/substitui notificações com a mesma tag (evita empilhar duplicadas).</summary>
    public string? Tag { get; set; }
}
