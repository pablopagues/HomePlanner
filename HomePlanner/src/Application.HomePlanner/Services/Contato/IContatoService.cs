using Application.HomePlanner.DTOs.Contato;

namespace Application.HomePlanner.Services.Contato;

public interface IContatoService
{
    Task<ContatoEnvioResultado> EnviarAsync(ContatoMensagemDTO mensagem, CancellationToken ct = default);
}

public class ContatoEnvioResultado
{
    public bool Sucesso { get; set; }
    public string? Erro { get; set; }

    public static ContatoEnvioResultado Ok() => new() { Sucesso = true };
    public static ContatoEnvioResultado Falha(string erro) => new() { Sucesso = false, Erro = erro };
}
