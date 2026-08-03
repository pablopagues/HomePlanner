using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Auth;

/// <summary>Credenciais enviadas pelo app mobile em POST /api/auth/login.</summary>
public class LoginApiDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;

    /// <summary>Identificação opcional do dispositivo (modelo/SO), só para auditoria.</summary>
    public string? DispositivoInfo { get; set; }
}
