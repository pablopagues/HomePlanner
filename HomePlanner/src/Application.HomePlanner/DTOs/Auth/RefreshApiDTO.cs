using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Auth;

/// <summary>Corpo de POST /api/auth/refresh — troca um refresh token válido por um novo par.</summary>
public class RefreshApiDTO
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public string? DispositivoInfo { get; set; }
}
