namespace Application.HomePlanner.DTOs.Planner;

public class MembroFamiliaDTO
{
    public string UsuarioId { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public DateTime? FotoAtualizadaEm { get; init; }

    public string? FotoVersao => FotoAtualizadaEm?.Ticks.ToString();
}
