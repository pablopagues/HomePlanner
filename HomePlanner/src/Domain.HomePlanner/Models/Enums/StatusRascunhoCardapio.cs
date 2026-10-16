namespace Domain.HomePlanner.Models.Enums;

public enum StatusRascunhoCardapio
{
    /// <summary>Geração em andamento ou a repetir (erro temporário da IA).</summary>
    Gerando = 0,
    /// <summary>Pronto, aguardando a família revisar.</summary>
    Pronto = 1,
    Aplicado = 2,
    Descartado = 3,
    /// <summary>Nada a fazer (ex.: a semana já estava toda planejada).</summary>
    Ignorado = 4,
    /// <summary>Não foi possível gerar (cota, sem receitas, tentativas esgotadas).</summary>
    Falhou = 5,
}
