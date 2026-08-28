using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Perfil;

/// <summary>Idioma preferido do usuário, em código curto (pt, en, es, fr).</summary>
public class DefinirIdiomaDTO
{
    [Required]
    [StringLength(5)]
    public string Idioma { get; set; } = string.Empty;
}
