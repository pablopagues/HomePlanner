namespace Domain.HomePlanner.Models.SaaS.Options;

/// <summary>Configuração do formulário público de contato (seção "Contato").</summary>
public class ContatoOptions
{
    public const string SectionName = "Contato";

    public string? EmailDestino { get; set; }
    public string? NomeDestino { get; set; }
}
