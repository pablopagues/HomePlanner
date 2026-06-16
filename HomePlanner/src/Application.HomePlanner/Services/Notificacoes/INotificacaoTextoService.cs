namespace Application.HomePlanner.Services.Notificacoes;

/// <summary>
/// Monta o texto das notificações no idioma do destinatário, sem depender de uma requisição
/// (o serviço de push e o background usam isto). <paramref name="idioma"/> é pt/en/es; null = padrão do app.
/// </summary>
public interface INotificacaoTextoService
{
    (string Titulo, string Corpo) LembreteTarefa(string? idioma, string tituloTarefa, TimeOnly hora);

    /// <summary>Lembrete enviado aos pais (Owner/Membro), identificando o responsável pela tarefa.</summary>
    (string Titulo, string Corpo) LembreteTarefaPais(string? idioma, string tituloTarefa, string nomeResponsavel, TimeOnly hora);

    (string Titulo, string Corpo) TarefaAtribuida(string? idioma, string tituloTarefa);
}
