namespace Application.HomePlanner.DTOs.Cardapio.Receita;

public class ReceitaFiltroDTO
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public string? TextoBusca { get; set; }
}
