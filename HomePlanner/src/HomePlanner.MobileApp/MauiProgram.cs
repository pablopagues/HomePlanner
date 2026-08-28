using HomePlanner.MobileApp.Services;
using Microsoft.Extensions.Logging;

namespace HomePlanner.MobileApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Traduções compartilhadas com a web (projeto Resources.HomePlanner).
		// ResourcesPath tem que ser o mesmo usado lá, senão o localizador devolve a chave.
		builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");

		// ── HomePlanner API ──
		builder.Services.AddSingleton<SessaoAtual>();
		builder.Services.AddSingleton<LinksLegais>();
		builder.Services.AddSingleton<IdiomaService>();
		builder.Services.AddSingleton<ErroApi>();
		builder.Services.AddTransient<AuthMessageHandler>();
		builder.Services.AddTransient<IdiomaMessageHandler>();

		// Client de auth (login/2fa/refresh/registro) — NÃO usa o AuthMessageHandler.
		// Leva o de idioma: as mensagens de erro de login também precisam sair traduzidas.
		builder.Services.AddHttpClient<AuthApi>()
#if DEBUG
			.ConfigurePrimaryHttpMessageHandler(CriarHandlerDev)
#endif
			.AddHttpMessageHandler<IdiomaMessageHandler>();

		// Client autenticado (demais chamadas) — Bearer + refresh via AuthMessageHandler.
		builder.Services.AddHttpClient<ApiClient>()
#if DEBUG
			.ConfigurePrimaryHttpMessageHandler(CriarHandlerDev)
#endif
			.AddHttpMessageHandler<IdiomaMessageHandler>()
			.AddHttpMessageHandler<AuthMessageHandler>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Resolve o IdiomaService já na subida: o construtor aplica a cultura no processo,
		// e isso precisa acontecer antes da primeira tela renderizar. Adiado, a primeira
		// tela sairia em português mesmo com outro idioma escolhido.
		app.Services.GetRequiredService<IdiomaService>();

		return app;
	}

#if DEBUG
	// DEBUG apenas: aceita o certificado de dev (https://localhost / self-signed) para testes locais.
	private static HttpMessageHandler CriarHandlerDev() => new HttpClientHandler
	{
		ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
	};
#endif
}
