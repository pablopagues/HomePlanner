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

		// ── HomePlanner API ──
		builder.Services.AddSingleton<SessaoAtual>();
		builder.Services.AddTransient<AuthMessageHandler>();

		// Client de auth (login/2fa/refresh/registro) — NÃO usa o AuthMessageHandler.
		builder.Services.AddHttpClient<AuthApi>()
#if DEBUG
			.ConfigurePrimaryHttpMessageHandler(CriarHandlerDev)
#endif
			;

		// Client autenticado (demais chamadas) — Bearer + refresh via AuthMessageHandler.
		builder.Services.AddHttpClient<ApiClient>()
#if DEBUG
			.ConfigurePrimaryHttpMessageHandler(CriarHandlerDev)
#endif
			.AddHttpMessageHandler<AuthMessageHandler>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

#if DEBUG
	// DEBUG apenas: aceita o certificado de dev (https://localhost / self-signed) para testes locais.
	private static HttpMessageHandler CriarHandlerDev() => new HttpClientHandler
	{
		ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
	};
#endif
}
