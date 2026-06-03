using System.ComponentModel.DataAnnotations;

namespace Application.HomePlanner.DTOs.Empresa;

public class AtualizarEmpresaDTO
{
    [Required(ErrorMessage = "Informe o nome do responsável.")]
    [MaxLength(200, ErrorMessage = "Nome muito longo.")]
    public string NomeResponsavel { get; set; } = string.Empty;

    // ─── Brasil ───
    [MaxLength(14)]
    public string? Cpf { get; set; }

    // ─── Canadá ───
    [MaxLength(50)]
    public string? Province { get; set; }
}
