namespace Domain.HomePlanner.Models.SaaS.Configuracao;

public class Parametro
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataModificacao { get; set; }
}
