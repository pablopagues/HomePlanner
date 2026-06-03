namespace Application.HomePlanner.DTOs.Planner;

public class TarefaFiltroDTO
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 50;
    public string? TextoBusca { get; set; }
    public bool? Concluida { get; set; }
    public string? ResponsavelUsuarioId { get; set; }

    /// <summary>Início do intervalo de datas (inclusive) — usado pelo calendário.</summary>
    public DateOnly? DataDe { get; set; }
    /// <summary>Fim do intervalo de datas (inclusive) — usado pelo calendário.</summary>
    public DateOnly? DataAte { get; set; }
}
