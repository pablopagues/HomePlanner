using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Contato;

public class ContatoMensagemDTO
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail é muito longo.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(40, ErrorMessage = "Telefone muito longo.")]
    public string? Telefone { get; set; }

    [Required(ErrorMessage = "Selecione um assunto.")]
    public string Assunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escreva sua mensagem.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A mensagem deve ter entre 10 e 2000 caracteres.")]
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Honeypot anti-bot: campo invisível; se vier preenchido, é bot.</summary>
    public string? Website { get; set; }
}
