using System.Globalization;
using System.Resources;
using Application.HomePlanner.Services.Notificacoes;
using Domain.HomePlanner.Models.SaaS.Options;
using Microsoft.Extensions.Options;
using Resources.HomePlanner;

namespace HomePlanner.BlazorServer.Services;

/// <summary>
/// Resolve os textos das notificações via as SharedResource.resx, escolhendo a cultura
/// explicitamente (ResourceManager) — não depende de CurrentUICulture/thread.
/// </summary>
public class NotificacaoTextoService : INotificacaoTextoService
{
    // Nome-base derivado do próprio tipo: se as .resx mudarem de projeto outra vez,
    // isto acompanha em vez de falhar em silêncio no runtime.
    private static readonly ResourceManager Rm =
        new($"{typeof(SharedResource).Assembly.GetName().Name}.Resources.SharedResource",
            typeof(SharedResource).Assembly);

    private readonly string _idiomaPadrao;

    public NotificacaoTextoService(IOptions<LocalizacaoOptions> opts)
        => _idiomaPadrao = opts.Value.IdiomaPadrao;

    public (string Titulo, string Corpo) LembreteTarefa(string? idioma, string tituloTarefa, TimeOnly hora)
    {
        var c = Cultura(idioma);
        var corpo = string.Format(c,
            Rm.GetString("Push_Lembrete_Corpo", c) ?? "Começa às {0}.",
            hora.ToString("HH\\:mm", CultureInfo.InvariantCulture));
        return (tituloTarefa, corpo);
    }

    public (string Titulo, string Corpo) LembreteTarefaPais(string? idioma, string tituloTarefa, string nomeResponsavel, TimeOnly hora)
    {
        var c = Cultura(idioma);
        var corpo = string.Format(c,
            Rm.GetString("Push_Lembrete_Pais_Corpo", c) ?? "Tarefa de {0} · começa às {1}.",
            nomeResponsavel,
            hora.ToString("HH\\:mm", CultureInfo.InvariantCulture));
        return (tituloTarefa, corpo);
    }

    public (string Titulo, string Corpo) TarefaAtribuida(string? idioma, string tituloTarefa)
    {
        var c = Cultura(idioma);
        var titulo = Rm.GetString("Push_TarefaAtribuida_Titulo", c) ?? "Nova tarefa para você";
        var corpo = string.Format(c,
            Rm.GetString("Push_TarefaAtribuida_Corpo", c) ?? "Você recebeu: {0}",
            tituloTarefa);
        return (titulo, corpo);
    }

    private CultureInfo Cultura(string? idioma)
    {
        var lang = PublicLanguageService.IsLangValida(idioma) ? idioma! : _idiomaPadrao;
        return PublicLanguageService.LangParaCulture(lang);
    }
}
