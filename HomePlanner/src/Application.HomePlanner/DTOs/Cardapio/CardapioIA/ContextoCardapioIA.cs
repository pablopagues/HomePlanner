using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.CardapioIA;

/// <summary>Receita da casa como candidata ao cardápio (vinda do repositório).</summary>
public class ReceitaCandidataCardapioDTO
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public int? TempoPreparoMinutos { get; init; }

    /// <summary>Primeiros ingredientes estruturados (na ordem da receita).</summary>
    public IReadOnlyList<string> Ingredientes { get; init; } = [];

    /// <summary>Ingredientes em texto livre (receitas ainda não "lançadas").</summary>
    public string? IngredientesTexto { get; init; }

    /// <summary>É componente de algum prato composto (ex.: "Arroz branco").</summary>
    public bool EhComponente { get; init; }
}

/// <summary>Uma refeição já planejada (histórico ou semana atual).</summary>
public record RefeicaoHistoricoDTO(DateOnly Data, TipoRefeicao Tipo, int ReceitaId, string? ReceitaNome);

/// <summary>Tudo que a IA recebe para montar o cardápio — já filtrado pelas regras rígidas.</summary>
public class ContextoCardapioIA
{
    public IReadOnlyList<CandidataCardapioIA> Candidatas { get; init; } = [];
    public IReadOnlyList<HorarioCardapioIA> Horarios { get; init; } = [];

    /// <summary>Pratos que continuam na semana (não serão trocados).</summary>
    public IReadOnlyList<RefeicaoHistoricoDTO> JaNaSemana { get; init; } = [];

    /// <summary>O que a família comeu nas últimas semanas (para variar).</summary>
    public IReadOnlyList<RefeicaoHistoricoDTO> Recentes { get; init; } = [];

    public string? Pedido { get; init; }
    public int Porcoes { get; init; }
    public bool PermitirRepetirNaSemana { get; init; }
}

public class CandidataCardapioIA
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public int? TempoPreparoMinutos { get; init; }
    public IReadOnlyList<string> Ingredientes { get; init; } = [];

    /// <summary>Quantas vezes foi usada em cada tipo de refeição nos últimos meses.</summary>
    public IReadOnlyDictionary<TipoRefeicao, int> UsosPorTipo { get; init; } = new Dictionary<TipoRefeicao, int>();
    public DateOnly? UltimoUso { get; init; }
}

public class HorarioCardapioIA
{
    public DiaSemana Dia { get; init; }
    public DateOnly Data { get; init; }
    public TipoRefeicao Tipo { get; init; }
    public int? TempoMaxMinutos { get; init; }
}

/// <summary>Resposta crua da IA (ainda não validada).</summary>
public class RespostaCardapioIA
{
    public IReadOnlyList<EscolhaCardapioIA> Escolhas { get; init; } = [];
    public string? Observacoes { get; init; }
}

public record EscolhaCardapioIA(DiaSemana Dia, TipoRefeicao Tipo, int ReceitaId, string? Motivo);
