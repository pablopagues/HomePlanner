using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Auth;

/// <summary>Passo 1 do "esqueci minha senha": dispara o e-mail com o link.</summary>
public class EsqueciSenhaDTO
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>Passo 2: redefine a senha com o token que chegou por e-mail.</summary>
public class RedefinirSenhaDTO
{
    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;
}

/// <summary>Troca de senha pelo próprio usuário, já autenticado.</summary>
public class AlterarSenhaDTO
{
    [Required(ErrorMessage = "Informe a senha atual.")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;
}
