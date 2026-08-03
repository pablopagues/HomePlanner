using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Notificacoes;

/// <summary>Dados do app para registrar um aparelho no push nativo (FCM).</summary>
public class RegistrarDispositivoDTO
{
    /// <summary>Token de registro FCM obtido no app.</summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>Plataforma ("android"/"ios") — opcional, para diagnóstico.</summary>
    public string? Plataforma { get; set; }

    /// <summary>Modelo/SO do aparelho — opcional, para diagnóstico.</summary>
    public string? DispositivoInfo { get; set; }
}
