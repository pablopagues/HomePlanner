using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Feedback;

/// <summary>Tipo do feedback enviado pelo usuário dentro do app.</summary>
public enum TipoFeedback
{
    Bug = 1,
    Sugestao = 2,
    Elogio = 3,
    Outro = 4,
}

/// <summary>
/// Feedback enviado pelo usuário via FAB dentro do app autenticado.
/// Recebido pelo FeedbackService, que envia e-mail para a equipe.
/// </summary>
public class FeedbackDTO
{
    [Required(ErrorMessage = "Selecione o tipo do feedback.")]
    public TipoFeedback Tipo { get; set; } = TipoFeedback.Sugestao;

    [Required(ErrorMessage = "Escreva sua mensagem.")]
    [StringLength(2000, MinimumLength = 10,
        ErrorMessage = "A mensagem deve ter entre 10 e 2000 caracteres.")]
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>URL da página onde estava ao abrir o feedback (auto-capturada).</summary>
    [StringLength(500)]
    public string? PaginaAtual { get; set; }
}
