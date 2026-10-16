using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.CardapioIA;

/// <summary>Janela em que uma receita já usada não pode voltar no cardápio gerado.</summary>
public enum PeriodoSemRepeticao
{
    Nenhum        = 0,
    DuasSemanas   = 2,
    TresSemanas   = 3,
    QuatroSemanas = 4,
    /// <summary>Meses-calendário cobertos pela semana (do dia 1º ao último dia).</summary>
    MesCorrente   = 99,
}

/// <summary>Parâmetros que a família escolhe para a IA montar o cardápio da semana.</summary>
public class PedidoCardapioIADTO
{
    /// <summary>Segunda-feira da semana a preencher.</summary>
    public DateOnly DataInicio { get; set; }

    public List<DiaSemana> Dias { get; set; } = [.. Enum.GetValues<DiaSemana>()];
    public List<TipoRefeicao> Tipos { get; set; } = [];

    /// <summary>false = só preenche horários vazios; true = também troca os que já têm prato.</summary>
    public bool SubstituirExistentes { get; set; }

    public PeriodoSemRepeticao NaoRepetir { get; set; } = PeriodoSemRepeticao.MesCorrente;

    /// <summary>Permite a mesma receita mais de uma vez na semana (ex.: sobra do jantar).</summary>
    public bool PermitirRepetirNaSemana { get; set; }

    public int? TempoMaxDiaUtil { get; set; }
    public int? TempoMaxFimDeSemana { get; set; }

    /// <summary>Porções por refeição; null = tamanho padrão da família.</summary>
    public int? Porcoes { get; set; }

    /// <summary>Pedido livre ("sem carne vermelha", "peixe na sexta").</summary>
    public string? Texto { get; set; }
}

/// <summary>Contagem prévia (sem IA, sem cota) para avisar quando faltam receitas.</summary>
public class PrevisaoCardapioIADTO
{
    public int Horarios { get; init; }
    public int ReceitasDisponiveis { get; init; }

    /// <summary>Há menos receitas que horários e não pode repetir: alguns ficarão vazios.</summary>
    public bool ReceitasInsuficientes { get; init; }
}
