using Application.HomePlanner.Extensions;
using Application.HomePlanner.Middleware;
using Application.HomePlanner.Repositories.Assinatura;
using Application.HomePlanner.Repositories.Cardapio;
using Application.HomePlanner.Repositories.Planner;
using Application.HomePlanner.Services.Assinatura;
using Application.HomePlanner.Services.Auth;
using Application.HomePlanner.Services.Cardapio;
using Application.HomePlanner.Services.Configuracao;
using Application.HomePlanner.Services.Contato;
using Application.HomePlanner.Services.Email;
using Application.HomePlanner.Services.Empresa;
using Application.HomePlanner.Services.Familia;
using Application.HomePlanner.Services.ListaCompras;
using Application.HomePlanner.Services.Onboarding;
using Application.HomePlanner.Services.Perfil;
using Application.HomePlanner.Services.Planner;
using Application.HomePlanner.Services.Seguranca;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Options;
using HomePlanner.BlazorServer.Services;
using Infrastructure.HomePlanner.Contexts;
using Infrastructure.HomePlanner.Repositories.Assinatura;
using Infrastructure.HomePlanner.Repositories.Cardapio;
using Infrastructure.HomePlanner.Repositories.Planner;
using Infrastructure.HomePlanner.Services.Auth;
using Infrastructure.HomePlanner.Services.Configuracao;
using Infrastructure.HomePlanner.Services.Contato;
using Infrastructure.HomePlanner.Services.Email;
using Infrastructure.HomePlanner.Services.Empresa;
using Infrastructure.HomePlanner.Services.Familia;
using Infrastructure.HomePlanner.Services.Onboarding;
using Infrastructure.HomePlanner.Services.Perfil;
using Infrastructure.HomePlanner.Services.Seguranca;
using Infrastructure.HomePlanner.Services.Stripe;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;

// ── Bootstrap logger (antes de qualquer coisa) ──────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando HomePlanner...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────────
    var serilogConfig = new ConfigurationBuilder()
        .SetBasePath(Path.Combine(AppContext.BaseDirectory, "ConfigLogs"))
        .AddJsonFile("Serilog.json", optional: false)
        .AddJsonFile($"Serilog.{builder.Environment.EnvironmentName}.json", optional: true)
        .Build();

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(serilogConfig)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ── HttpContextAccessor ──────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();

    // ── Options ──────────────────────────────────────────────────────────────
    builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
    builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));
    builder.Services.Configure<GmailOptions>(builder.Configuration.GetSection(GmailOptions.SectionName));
    builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
    builder.Services.Configure<LocalizacaoOptions>(builder.Configuration.GetSection(LocalizacaoOptions.SectionName));

    // ── Localization (.resx em Resources/, 3 idiomas) ─────────────────────────
    builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
    builder.Services.AddScoped<PublicLanguageService>();
    builder.Services.AddScoped<EstadoFotoUsuario>();

    // ── TenantContext ─────────────────────────────────────────────────────────
    builder.Services.AddTenantContext();

    // ── Database (DbContextFactory padrão Blazor Server) ─────────────────────
    var connStr = builder.Configuration.GetConnectionString("SqlConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:SqlConnection não configurado.");

    builder.Services.AddDbContextFactory<HomePlannerDbContext>(
        (sp, opts) => opts.UseSqlServer(connStr),
        lifetime: ServiceLifetime.Scoped);

    builder.Services.AddScoped<HomePlannerDbContext>(sp =>
        sp.GetRequiredService<IDbContextFactory<HomePlannerDbContext>>().CreateDbContext());

    // ── Identity ──────────────────────────────────────────────────────────────
    builder.Services
        .AddIdentity<Usuario, Papel>(opts =>
        {
            opts.Password.RequiredLength = 8;
            opts.Password.RequireDigit = true;
            opts.Password.RequireLowercase = true;
            opts.Password.RequireUppercase = true;
            opts.Password.RequireNonAlphanumeric = true;

            opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opts.Lockout.MaxFailedAccessAttempts = 5;
            opts.Lockout.AllowedForNewUsers = true;

            opts.User.RequireUniqueEmail = true;
            // Login é permitido sem confirmar e-mail; um aviso é exibido no Dashboard
            // e o reenvio fica disponível na página /empresa (dados da conta).
            opts.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<HomePlannerDbContext>()
        .AddDefaultTokenProviders();

    // Claims factory customizado (adiciona tenant_id + NomeAmigavel)
    builder.Services.AddScoped<IUserClaimsPrincipalFactory<Usuario>, TenantUserClaimsPrincipalFactory>();

    // ── Cookie Auth ───────────────────────────────────────────────────────────
    builder.Services.ConfigureApplicationCookie(opts =>
    {
        opts.LoginPath = "/Identity/Account/Login";
        opts.LogoutPath = "/Identity/Account/Logout";
        opts.AccessDeniedPath = "/Identity/Account/AccessDenied";
        opts.ExpireTimeSpan = TimeSpan.FromDays(7);
        opts.SlidingExpiration = true;
    });

    // 2FA: "lembrar este dispositivo por 30 dias" — duração do cookie TwoFactorRememberMe.
    builder.Services.Configure<CookieAuthenticationOptions>(
        IdentityConstants.TwoFactorRememberMeScheme,
        opts => opts.ExpireTimeSpan = TimeSpan.FromDays(30));

    // ── Authorization Policies ────────────────────────────────────────────────
    builder.Services.AddAuthorization(PoliciesAutorizacao.Registrar);

    // ── AutoMapper ────────────────────────────────────────────────────────────
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // ── Razor Pages + Controllers + Blazor + MudBlazor ───────────────────────
    builder.Services.AddRazorPages();
    builder.Services.AddControllers(); // webhook do Stripe
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    builder.Services.AddMudServices();

    // ── Cardápio: Repositórios ────────────────────────────────────────────────
    builder.Services.AddScoped<IUnidadeMedidaRepository, UnidadeMedidaRepository>();
    builder.Services.AddScoped<IIngredienteRepository, IngredienteRepository>();
    builder.Services.AddScoped<IReceitaRepository, ReceitaRepository>();
    builder.Services.AddScoped<IPlanejamentoSemanalRepository, PlanejamentoSemanalRepository>();
    builder.Services.AddScoped<IModeloSemanaRepository, ModeloSemanaRepository>();

    // ── Cardápio: Serviços ────────────────────────────────────────────────────
    builder.Services.AddScoped<IConversorUnidadeService, ConversorUnidadeService>();
    builder.Services.AddScoped<IIngredienteService, IngredienteService>();
    builder.Services.AddScoped<IReceitaService, ReceitaService>();
    builder.Services.AddScoped<ICardapioService, CardapioService>();
    builder.Services.AddScoped<IModeloSemanaService, ModeloSemanaService>();

    // ── Importador de receitas (via URL) ──────────────────────────────────────
    builder.Services.AddHttpClient<IImportadorReceitaService, ImportadorReceitaService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });

    // ── Lista de Compras ──────────────────────────────────────────────────────
    builder.Services.AddScoped<IListaComprasService, ListaComprasService>();

    // ── Planner / Tarefas ─────────────────────────────────────────────────────
    builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
    builder.Services.AddScoped<ITarefaService, TarefaService>();

    // ── Auth / Onboarding / Email ─────────────────────────────────────────────
    builder.Services.AddScoped<IRegistroTenantService, RegistroTenantService>();
    builder.Services.AddScoped<IOnboardingService, OnboardingService>();
    builder.Services.AddScoped<IConfiguracaoFamiliaService, ConfiguracaoFamiliaService>();
    builder.Services.AddScoped<IFamiliaService, FamiliaService>();
    builder.Services.AddScoped<IDoisFatoresService, DoisFatoresService>();
    builder.Services.AddScoped<IFotoUsuarioService, FotoUsuarioService>();
    builder.Services.AddScoped<IOnboardingStatusReader, OnboardingStatusReader>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IEmpresaService, EmpresaService>();

    // ── Contato (formulário público) ──────────────────────────────────────────
    builder.Services.Configure<ContatoOptions>(builder.Configuration.GetSection(ContatoOptions.SectionName));
    builder.Services.AddScoped<IContatoService, ContatoService>();

    // ── Feedback (formulário in-app) ──────────────────────────────────────────
    builder.Services.AddScoped<Application.HomePlanner.Services.Feedback.IFeedbackService,
                               Infrastructure.HomePlanner.Services.Feedback.FeedbackService>();

    // ── Stripe / Assinatura ───────────────────────────────────────────────────
    builder.Services.AddScoped<IAssinaturaRepository, AssinaturaRepository>();
    builder.Services.AddScoped<IStripeBillingService, StripeBillingService>();
    builder.Services.AddScoped<IStripeWebhookHandler, StripeWebhookHandler>();
    builder.Services.AddScoped<IAssinaturaService, AssinaturaService>();

    // ═══════════════════════════════════════════════════════════════════════════
    var app = builder.Build();
    // ═══════════════════════════════════════════════════════════════════════════

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // ── Localization — PublicLangCookieProvider é o único árbitro ────────────
    app.UseRequestLocalization(opts =>
    {
        var suportados = new[] { "pt-BR", "en", "es" };
        opts.SetDefaultCulture("pt-BR")
            .AddSupportedCultures(suportados)
            .AddSupportedUICultures(suportados);

        opts.RequestCultureProviders.Clear();
        opts.RequestCultureProviders.Add(new PublicLangCookieProvider());
    });

    // ── Troca de idioma (GET /set-lang?lang=pt|en|es&returnUrl=/) ────────────
    app.MapGet("/set-lang", (string? lang, string? returnUrl, HttpContext ctx) =>
    {
        if (PublicLanguageService.IsLangValida(lang))
        {
            ctx.Response.Cookies.Append(PublicLanguageService.CookieName, lang!, new CookieOptions
            {
                MaxAge = TimeSpan.FromDays(365),
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Path = "/"
            });
        }

        var redirect = returnUrl ?? "/";
        if (!redirect.StartsWith('/') || redirect.StartsWith("//")) redirect = "/";
        return Results.Redirect(redirect);
    }).AllowAnonymous();

    app.UseRouting();
    app.UseAuthentication();
    app.UseTenantContext();
    app.UseOnboardingRequired();
    app.UseAuthorization();
    app.UseAntiforgery();

    // ── Foto de perfil do usuário atual (GET /perfil/foto) ───────────────────
    app.MapGet("/perfil/foto", async (IFotoUsuarioService svc, CancellationToken ct) =>
    {
        var foto = await svc.ObterConteudoFotoAsync(ct);
        if (foto is null) return Results.NotFound();

        var etag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{foto.Versao}\"");
        return Results.File(foto.Conteudo, foto.ContentType, entityTag: etag);
    }).RequireAuthorization();

    // ── Foto de um membro da família (GET /usuario/{usuarioId}/foto) ─────────
    // O serviço aplica o filtro de tenant: só serve fotos de membros da própria família.
    app.MapGet("/usuario/{usuarioId}/foto", async (string usuarioId, IFotoUsuarioService svc, CancellationToken ct) =>
    {
        var foto = await svc.ObterConteudoFotoAsync(usuarioId, ct);
        if (foto is null) return Results.NotFound();

        var etag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{foto.Versao}\"");
        return Results.File(foto.Conteudo, foto.ContentType, entityTag: etag);
    }).RequireAuthorization();

    app.MapRazorPages();
    app.MapControllers(); // StripeWebhookController
    app.MapRazorComponents<HomePlanner.BlazorServer.Components.App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha fatal ao iniciar HomePlanner");
}
finally
{
    Log.CloseAndFlush();
}
