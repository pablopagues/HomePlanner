namespace Application.HomePlanner.DTOs.Onboarding;

public class ConfiguracaoFamiliaDTO
{
    public int TamanhoFamiliaPadrao { get; set; } = 4;
    public string FusoHorario { get; set; } = "America/Toronto";
    public int MinutosAntecedenciaLembrete { get; set; } = 15;
    public List<string> TiposRefeicaoAtivos { get; set; } = ["Almoco"];
}
