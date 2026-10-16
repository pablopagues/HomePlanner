namespace Application.HomePlanner.DTOs.Cardapio.Receita;

/// <summary>O que a família quer que a IA sugira.</summary>
public class PedidoSugestaoReceitaDTO
{
    /// <summary>Pedido livre ("jantar rápido com frango, sem lactose"). Vazio = surpreenda-me.</summary>
    public string? Texto { get; set; }
    public int? TempoMaximoMinutos { get; set; }
    public int? Porcoes { get; set; }
    /// <summary>Envia os nomes das receitas da casa como contexto (gosto + não repetir).</summary>
    public bool ConsiderarReceitasDaCasa { get; set; } = true;
    /// <summary>Nomes já sugeridos nesta sessão — a IA evita repeti-los.</summary>
    public List<string> JaSugeridas { get; set; } = [];
}
