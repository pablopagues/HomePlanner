using Domain.HomePlanner.Models.SaaS.Interfaces;

namespace Domain.HomePlanner.Models.Cardapio;

public class Receita : ITenantEntity, IDeletableEntity, IAuditable
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NomeNormalizado { get; set; } = string.Empty;
    public string? ModoPreparo { get; set; }
    public int NumeroPorcoesBase { get; set; } = 4;
    public int? TempoPreparoMinutos { get; set; }
    public string? UrlOrigem { get; set; }
    public string? UrlImagem { get; set; }
    public string? Observacoes { get; set; }

    // Foto enviada pelo usuário (uma por receita), já redimensionada/comprimida.
    // Tem precedência sobre UrlImagem na exibição. UrlImagem continua servindo
    // receitas importadas de sites (imagem hotlinkada).
    public byte[]? Foto { get; set; }
    public string? FotoContentType { get; set; }
    public DateTime? FotoAtualizadaEm { get; set; }

    // IDeletableEntity
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUsuarioId { get; set; }

    // IAuditable
    public DateTime DataCriacao { get; set; }
    public DateTime? DataModificacao { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }

    // Navigation
    public ICollection<ReceitaIngrediente> Ingredientes { get; set; } = [];
    public ICollection<ReceitaComponente> Componentes { get; set; } = [];
    public ICollection<RefeicaoDia> RefeicoesDia { get; set; } = [];
    public ICollection<RefeicaoModelo> RefeicoesModelo { get; set; } = [];
}
