namespace Application.HomePlanner.DTOs.Familia;

public class ResumoFamiliaDTO
{
    public int MembrosAtivos { get; init; }
    public int LimiteMembros { get; init; }

    public bool LimiteAtingido => MembrosAtivos >= LimiteMembros;
}
