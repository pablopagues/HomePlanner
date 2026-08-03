using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Auth;

/// <summary>Segundo passo do login com 2FA: valida o código e emite os tokens.</summary>
public class Confirmar2FADTO
{
    [Required]
    public string MfaToken { get; set; } = string.Empty;

    /// <summary>Código do app autenticador (TOTP) ou, se <see cref="CodigoRecuperacao"/>, um código de recuperação.</summary>
    [Required]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Quando true, <see cref="Codigo"/> é interpretado como código de recuperação.</summary>
    public bool CodigoRecuperacao { get; set; }

    public string? DispositivoInfo { get; set; }
}
