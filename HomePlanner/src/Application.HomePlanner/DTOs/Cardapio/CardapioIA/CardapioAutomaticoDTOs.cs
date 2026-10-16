using Domain.HomePlanner.Models.Enums;

namespace Application.HomePlanner.DTOs.Cardapio.CardapioIA;

/// <summary>Rascunho automático: quando a IA prepara a semana seguinte e com quais regras.</summary>
public class AgendamentoCardapioIADTO
{
    public bool Ativo { get; set; }
    public DiaSemana DiaSemana { get; set; } = DiaSemana.Sexta;

    /// <summary>Hora local da família (0–23).</summary>
    public int Hora { get; set; } = 9;

    /// <summary>Regras do cardápio (a data é ignorada: vale sempre a semana seguinte).</summary>
    public PedidoCardapioIADTO Regras { get; set; } = new();
}

/// <summary>Rascunho que a IA preparou sozinha, aguardando revisão.</summary>
public class RascunhoCardapioIADTO
{
    public int Id { get; init; }
    public DateOnly DataInicio { get; init; }
    public DateTime GeradoEm { get; init; }
    public CardapioSugeridoDTO Sugestao { get; init; } = new();
}

/// <summary>O que aconteceu numa tentativa de rascunho automático (para o serviço de fundo).</summary>
public enum ResultadoRascunhoAutomatico
{
    /// <summary>Agendamento desligado, rascunho já existe ou outra tentativa recente.</summary>
    NadaAFazer,
    /// <summary>Rascunho pronto: avisar a família.</summary>
    Pronto,
    /// <summary>A semana já estava planejada.</summary>
    Ignorado,
    /// <summary>Erro temporário da IA; tenta de novo num próximo ciclo.</summary>
    TentarDepois,
    /// <summary>Não deu (cota, sem receitas, tentativas esgotadas).</summary>
    Falhou,
}
