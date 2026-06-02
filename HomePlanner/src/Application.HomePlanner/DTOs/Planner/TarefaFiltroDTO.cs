namespace Application.HomePlanner.DTOs.Planner;

public class TarefaFiltroDTO
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 50;
    public string? TextoBusca { get; set; }
    public bool? Concluida { get; set; }
    public string? ResponsavelUsuarioId { get; set; }
}
