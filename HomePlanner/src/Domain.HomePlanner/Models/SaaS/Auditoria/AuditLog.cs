namespace Domain.HomePlanner.Models.SaaS.Auditoria;

public class AuditLog
{
    public long Id { get; set; }
    public Guid? TenantId { get; set; }
    public string? UsuarioId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Entidade { get; set; }
    public string? EntidadeId { get; set; }
    public string? DadosAntes { get; set; }
    public string? DadosDepois { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime DataHora { get; set; }
}
