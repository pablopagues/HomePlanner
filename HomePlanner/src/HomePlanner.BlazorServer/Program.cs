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
using Application.HomePlanner.Services.Notificacoes;
using Application.HomePlanner.Services.Onboarding;
using Application.HomePlanner.Services.Perfil;
using Application.HomePlanner.Services.Planner;
using Application.HomePlanner.Services.Seguranca;
using Domain.HomePlanner.Models.SaaS.Identity;
using Domain.HomePlanner.Models.SaaS.Options;
using Google.Apis.Auth.OAuth2;
using HomePlanner.BlazorServer.Services;
using Infrastructure.HomePlanner.Contexts;
using Infrastructure.HomePlanner.Repositories.Assinatura;
using Infrastructure.HomePlanner.Repositories.Cardapio;
using Infrastructure.HomePlanner.Repositories.Planner;
using Infrastructure.HomePlanner.Services.Assinatura;
using Infrastructure.HomePlanner.Services.Auth;
using Infrastructure.HomePlanner.Services.Cardapio;
using Infrastructure.HomePlanner.Services.Configuracao;
using Infrastructure.HomePlanner.Services.Contato;
using Infrastructure.HomePlanner.Services.Email;
using Infrastructure.HomePlanner.Services.Empresa;
using Infrastructure.HomePlanner.Services.Familia;
using Infrastructure.HomePlanner.Services.Identity;
using Infrastructure.HomePlanner.Services.Notificacoes;
using Infrastructure.HomePlanner.Services.Onboarding;
using Infrastructure.HomePlanner.Services.Perfil;
using Infrastructure.HomePlanner.Services.Seguranca;
using Infrastructure.HomePlanner.Services.Stripe;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
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

    // Até builder.Build() só existe o bootstrap logger (console): o que for logado antes
    // disso nunca chega ao arquivo em logs/. Os diagnósticos de inicialização ficam
    // guardados aqui e são emitidos logo após o Build, já com o sink de arquivo ativo.
    var logsInicio = new List<Action>();

    // ── HttpContextAccessor ──────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();

    // ── Data Protection ──────────────────────────────────────────────────────
    // Sem isto, o ASP.NET Core guarda as chaves só em memória quando o app pool não
    // tem perfil de usuário carregado — o caso da hospedagem compartilhada. A cada
    // reciclo do pool as chaves mudam, e todo cookie de autenticação e token
    // antiforgery emitidos antes param de descriptografar: o usuário cai da sessão e
    // o POST de login falha com "The antiforgery token could not be decrypted".
    var pastaChaves = builder.Configuration["DataProtection:KeysPath"];
    if (string.IsNullOrWhiteSpace(pastaChaves))
        pastaChaves = Path.Combine("App_Data", "keys");
    if (!Path.IsPathRooted(pastaChaves))
        pastaChaves = Path.Combine(builder.Environment.ContentRootPath, pastaChaves);

    Directory.CreateDirectory(pastaChaves);

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(pastaChaves))
        // Nome fixo de propósito: o padrão deriva do caminho físico da aplicação, que
        // muda entre deploys na hospedagem e invalidaria as chaves sem necessidade.
        .SetApplicationName("HomePlanner");

    logsInicio.Add(() => Log.Information("Data Protection: chaves em {Pasta}", pastaChaves));

    // ── Options ──────────────────────────────────────────────────────────────
    builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
    builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));
    builder.Services.Configure<GmailOptions>(builder.Configuration.GetSection(GmailOptions.SectionName));
    builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
    builder.Services.Configure<LocalizacaoOptions>(builder.Configuration.GetSection(LocalizacaoOptions.SectionName));
    builder.Services.Configure<WebPushOptions>(builder.Configuration.GetSection(WebPushOptions.SectionName));
    builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection(AnthropicOptions.SectionName));
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
    builder.Services.Configure<FcmOptions>(builder.Configuration.GetSection(FcmOptions.SectionName));

    // ── Localization (.resx em Resources/, 3 idiomas) ─────────────────────────
    builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
    builder.Services.AddScoped<PublicLanguageService>();
    builder.Services.AddScoped<EstadoFotoUsuario>();
    builder.Services.AddScoped<EstadoMural>();

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
        .AddDefaultTokenProviders()
        // Convite de membro usa provider próprio (validade de 7 dias, contra 1 dia do padrão).
        .AddTokenProvider<ConviteTokenProvider<Usuario>>(ConviteToken.Provider);

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

        // A cada 30 min o Identity revalida o cookie buscando o usuário no banco. Isso
        // roda na autenticação, antes do TenantContextMiddleware: sem hidratar o tenant
        // aqui, o filtro global (Usuario é ITenantEntity) esconde o usuário, a validação
        // falha e ele é deslogado — ao voltar de uma aba inativa, por exemplo.
        opts.Events.OnValidatePrincipal = contexto =>
        {
            contexto.HttpContext.RequestServices.GetRequiredService<TenantContext>()
                .DefinirAPartirDe(contexto.Principal);
            return SecurityStampValidator.ValidatePrincipalAsync(contexto);
        };
    });

    // 2FA: "lembrar este dispositivo por 30 dias" — duração do cookie TwoFactorRememberMe.
    builder.Services.Configure<CookieAuthenticationOptions>(
        IdentityConstants.TwoFactorRememberMeScheme,
        opts => opts.ExpireTimeSpan = TimeSpan.FromDays(30));

    // ── JWT Bearer (API mobile) ────────────────────────────────────────────────
    // Esquema ADICIONAL — o cookie continua sendo o esquema padrão do Blazor.
    // A API opta pelo Bearer via [Authorize(AuthenticationSchemes = "Bearer")].
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    if (jwtOptions.EstaConfigurado)
    {
        builder.Services.AddAuthentication()
            .AddJwtBearer(opts =>
            {
                opts.MapInboundClaims = true; // "nameid"/"role" → ClaimTypes.* (igual ao cookie)
                opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };
            });

        // CORS liberado para o app (WebView/nativo). Ajuste as origens em produção se necessário.
        builder.Services.AddCors(o => o.AddPolicy("ApiMobile", p =>
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    builder.Services.AddScoped<IAuthTokenService, AuthTokenService>();

    // ── Authorization Policies ────────────────────────────────────────────────
    builder.Services.AddAuthorization(PoliciesAutorizacao.Registrar);

    // ── AutoMapper ────────────────────────────────────────────────────────────
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // ── Razor Pages + Controllers + Blazor + MudBlazor ───────────────────────
    builder.Services.AddRazorPages();
    // Controllers: webhook do Stripe + API mobile. Enums como string (JSON amigável para o app).
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
            opts.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter()));
    // Circuito: aba em segundo plano (sobretudo no celular) perde o WebSocket. O padrão
    // guarda o circuito desconectado por só 3 min — depois disso a volta exige recarregar.
    // Com 60 min, voltar à aba reconecta e mantém a tela como estava.
    // ClientTimeoutInterval maior tolera os pings atrasados de abas com timers
    // estrangulados pelo navegador (padrão de 30s derrubava a conexão à toa).
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents(opts =>
            opts.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(60))
        .AddHubOptions(opts =>
        {
            opts.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            opts.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });
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
    builder.Services.AddScoped<IImagemReceitaProcessor, ImagemReceitaProcessor>();
    builder.Services.AddScoped<ICardapioService, CardapioService>();
    builder.Services.AddScoped<IModeloSemanaService, ModeloSemanaService>();

    // ── Importador de receitas (via URL) ──────────────────────────────────────
    builder.Services.AddHttpClient<IImportadorReceitaService, ImportadorReceitaService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });

    // ── Parsing de ingredientes por IA (Claude) + cota de importação ──────────
    builder.Services.AddMemoryCache(); // usado pelo throttle anti-abuso da IA
    builder.Services.AddScoped<IParserIngredientesIA, ParserIngredientesIAService>();
    builder.Services.AddScoped<ITradutorReceitaIA, TradutorReceitaIAService>();
    builder.Services.AddScoped<ICotaImportacaoService, CotaImportacaoService>();

    // ── Lista de Compras ──────────────────────────────────────────────────────
    builder.Services.AddScoped<IListaComprasService, ListaComprasService>();
    builder.Services.AddScoped<IPedidoCompraRepository, PedidoCompraRepository>();
    builder.Services.AddScoped<IPedidoCompraService, PedidoCompraService>();
    builder.Services.AddScoped<IMarcacaoCompraRepository, MarcacaoCompraRepository>();
    builder.Services.AddScoped<IListaCompraRepository, ListaCompraRepository>();
    builder.Services.AddScoped<IListaCompraService, ListaCompraService>();
    builder.Services.AddScoped<IProdutoRecorrenteRepository, ProdutoRecorrenteRepository>();
    builder.Services.AddScoped<IProdutoRecorrenteService, ProdutoRecorrenteService>();

    // ── Planner / Tarefas ─────────────────────────────────────────────────────
    builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
    builder.Services.AddScoped<ITarefaService, TarefaService>();

    // ── Auth / Onboarding / Email ─────────────────────────────────────────────
    builder.Services.AddScoped<IRegistroTenantService, RegistroTenantService>();
    builder.Services.AddScoped<ISenhaService, SenhaService>();
    builder.Services.AddScoped<IOnboardingService, OnboardingService>();
    builder.Services.AddScoped<IConfiguracaoFamiliaService, ConfiguracaoFamiliaService>();
    builder.Services.AddScoped<IFamiliaService, FamiliaService>();
    builder.Services.AddScoped<IDoisFatoresService, DoisFatoresService>();
    builder.Services.AddScoped<IFotoUsuarioService, FotoUsuarioService>();
    builder.Services.AddScoped<IPreferenciasUsuarioService, PreferenciasUsuarioService>();
    builder.Services.AddScoped<IOnboardingStatusReader, OnboardingStatusReader>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IEmpresaService, EmpresaService>();

    // ── Notificações Push (Web Push / VAPID) ──────────────────────────────────
    builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
    // Push nativo (FCM) para os apps MAUI — convive com o Web Push no mesmo funil.
    builder.Services.AddScoped<IDispositivoPushService, DispositivoPushService>();

    // FirebaseApp default (uma vez) — só quando o FCM está configurado.
    //
    // Roda antes de builder.Build(), quando o Serilog de arquivo ainda não existe: uma
    // exceção aqui só apareceria no console e derrubaria o app inteiro por causa de um
    // recurso opcional. Por isso o try/catch — push é acessório, login não é.
    var fcmOptions = builder.Configuration.GetSection(FcmOptions.SectionName).Get<FcmOptions>() ?? new FcmOptions();

    // Sempre registra o estado: sem isto, "não configurado" e "configurado mas falhou"
    // produzem o mesmo silêncio no log, e não dá para saber se a variável de ambiente
    // sequer chegou na aplicação.
    if (!fcmOptions.IsEnabled)
    {
        logsInicio.Add(() => Log.Information(
            "Push nativo (FCM) desligado — Fcm:IsEnabled está false ou não chegou à aplicação."));
    }
    else if (string.IsNullOrWhiteSpace(fcmOptions.CredentialsJson)
             && string.IsNullOrWhiteSpace(fcmOptions.CredentialsPath))
    {
        logsInicio.Add(() => Log.Warning("Push nativo (FCM) ligado, mas sem credencial: " +
                                         "Fcm:CredentialsJson e Fcm:CredentialsPath estão vazios."));
    }
    else if (FirebaseAdmin.FirebaseApp.DefaultInstance is null)
    {
        // Caminho relativo resolve a partir da raiz da aplicação (onde fica o
        // appsettings.json). Em hospedagem compartilhada isso evita ter que descobrir o
        // caminho físico absoluto para preencher a variável de ambiente.
        var caminhoCredencial = fcmOptions.CredentialsPath;
        if (!string.IsNullOrWhiteSpace(caminhoCredencial) && !Path.IsPathRooted(caminhoCredencial))
            caminhoCredencial = Path.Combine(builder.Environment.ContentRootPath, caminhoCredencial);

        try
        {
            var credencial = !string.IsNullOrWhiteSpace(fcmOptions.CredentialsJson)
                ? GoogleCredential.FromJson(fcmOptions.CredentialsJson)
                : GoogleCredential.FromFile(caminhoCredencial!);

            FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions { Credential = credencial });
            logsInicio.Add(() => Log.Information("Firebase (FCM) inicializado — push nativo habilitado."));
        }
        catch (Exception ex)
        {
            // Diz qual fonte falhou e o suficiente para identificar a causa sem acesso ao
            // servidor: tamanho lido (denuncia JSON truncado ou mal escapado) ou caminho
            // resolvido e se o arquivo existe.
            var origem = !string.IsNullOrWhiteSpace(fcmOptions.CredentialsJson)
                ? $"Fcm:CredentialsJson ({fcmOptions.CredentialsJson!.Length} caracteres)"
                : $"Fcm:CredentialsPath ({caminhoCredencial}; existe: {File.Exists(caminhoCredencial)})";

            logsInicio.Add(() => Log.Error(ex,
                "FCM não pôde ser inicializado a partir de {Origem}. O push nativo fica " +
                "desligado; o resto da aplicação segue normalmente.", origem));
        }
    }
    // Textos das notificações no idioma do destinatário (lê as SharedResource.resx).
    builder.Services.AddSingleton<INotificacaoTextoService, HomePlanner.BlazorServer.Services.NotificacaoTextoService>();
    // Lembretes de tarefa por horário (varre o banco e dispara push na hora).
    builder.Services.AddHostedService<HomePlanner.BlazorServer.Services.LembreteTarefaBackgroundService>();

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
    // Guarda de acesso: trial vencido / pagamento em aberto bloqueiam o produto.
    builder.Services.AddScoped<IAssinaturaStatusReader, AssinaturaStatusReader>();

    // ═══════════════════════════════════════════════════════════════════════════
    var app = builder.Build();
    // ═══════════════════════════════════════════════════════════════════════════

    foreach (var emitir in logsInicio) emitir();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // Garante o MIME correto do manifest PWA (.webmanifest) — alguns hosts não o conhecem.
    var provedorTiposEstaticos = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
    provedorTiposEstaticos.Mappings[".webmanifest"] = "application/manifest+json";
    app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provedorTiposEstaticos });

    // ── Localization — PublicLangCookieProvider é o único árbitro ────────────
    app.UseRequestLocalization(opts =>
    {
        var suportados = new[] { "pt-BR", "en", "es", "fr" };
        opts.SetDefaultCulture("pt-BR")
            .AddSupportedCultures(suportados)
            .AddSupportedUICultures(suportados);

        opts.RequestCultureProviders.Clear();
        // Páginas: o cookie manda e encerra a cadeia.
        opts.RequestCultureProviders.Add(new PublicLangCookieProvider());
        // /api: o cookie se abstém e o idioma vem do Accept-Language do app.
        // Sem provider nenhum aqui, a API responderia sempre em pt-BR.
        opts.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
    });

    // ── Troca de idioma (GET /set-lang?lang=pt|en|es|fr&returnUrl=/) ─────────
    app.MapGet("/set-lang", async (string? lang, string? returnUrl, HttpContext ctx, HomePlannerDbContext db) =>
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

            // Usuário logado: persiste a escolha no perfil (usado pelas notificações fora de requisição).
            var usuarioId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(usuarioId))
            {
                var usuario = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == usuarioId);
                if (usuario is not null && usuario.Idioma != lang)
                {
                    usuario.Idioma = lang;
                    await db.SaveChangesAsync();
                }
            }
        }

        var redirect = returnUrl ?? "/";
        if (!redirect.StartsWith('/') || redirect.StartsWith("//")) redirect = "/";
        return Results.Redirect(redirect);
    }).AllowAnonymous();

    // ── Swagger da API (somente quando o JWT está configurado) ───────────────
    if (jwtOptions.EstaConfigurado)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    if (jwtOptions.EstaConfigurado)
        app.UseCors("ApiMobile");
    app.UseAuthentication();
    app.UseTenantContext();
    app.UseOnboardingRequired();
    app.UseAssinaturaRequired();
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

    // ── Foto de uma receita (GET /receita/{id}/foto) ─────────────────────────
    // O serviço aplica o filtro de tenant: só serve fotos de receitas da própria família.
    app.MapGet("/receita/{id:int}/foto", async (int id, IReceitaService svc, CancellationToken ct) =>
    {
        var foto = await svc.ObterFotoAsync(id, ct);
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
