# Instruções de Implementação — Temas Visuais HomePlanner

## Contexto

O projeto já possui o tema **Sage/Terracotta** implementado em:
- `HomePlanner.BlazorServer/Theme/HomePlannerTheme.cs` — tema MudBlazor (C#)
- `wwwroot/css/app.css` — variáveis CSS globais

Estas instruções adicionam suporte a **dois novos temas**:
- **Plum & Blush** — ameixa escura + rosa quartzo, sofisticado e feminino
- **Carbon & Mint** — carvão escuro + menta fresca, moderno e arrojado (dark-first)

---

## Paletas de Referência

### Plum & Blush

| Nome     | Hex       | Uso                          |
|----------|-----------|------------------------------|
| Plum     | `#7a4a9a` | Cor primária, botões, links  |
| Deep     | `#4a2a68` | Textos escuros, headers      |
| Blush    | `#e8a0b0` | Acento, badges, highlights   |
| Petal    | `#f5dce8` | Fundos de cards, hover       |
| Ivory    | `#fdf7fa` | Background principal         |
| Muted    | `#7a5a70` | Textos secundários           |

### Carbon & Mint

| Nome     | Hex       | Uso                          |
|----------|-----------|------------------------------|
| Carbon   | `#2a2e35` | Background principal (dark)  |
| Deep     | `#15181e` | Background secundário        |
| Mint     | `#4dc9a0` | Cor primária, botões, links  |
| Pale     | `#d0f0e4` | Acento claro, badges         |
| Light    | `#f4f5f6` | Textos principais (no dark)  |
| Muted    | `#9aa0ac` | Textos secundários           |

---

## Passo 1 — Atualizar `HomePlannerTheme.cs`

**Arquivo:** `HomePlanner.BlazorServer/Theme/HomePlannerTheme.cs`

**Instrução para o Claude:**

> Abre o arquivo `HomePlannerTheme.cs`. Ele já contém uma propriedade estática `Light` com o tema Sage/Terracotta.
>
> Adiciona duas novas propriedades estáticas `PlumBlush` e `CarbonMint` seguindo exatamente o mesmo padrão da propriedade `Light` existente. Não altera nada do tema existente.

O código a adicionar na classe `HomePlannerTheme`:

```csharp
/// <summary>
/// Tema Plum & Blush — ameixa escura + rosa quartzo.
/// Sofisticado, feminino, editorial.
/// </summary>
public static MudTheme PlumBlush => new()
{
    PaletteLight = new PaletteLight
    {
        Primary = "#7a4a9a",
        PrimaryDarken = "#4a2a68",
        PrimaryLighten = "#c8a0e0",
        PrimaryContrastText = "#fdf7fa",

        Secondary = "#e8a0b0",
        SecondaryDarken = "#c07080",
        SecondaryLighten = "#f5dce8",
        SecondaryContrastText = "#4a2a68",

        Tertiary = "#f5dce8",
        TertiaryContrastText = "#4a2a68",

        Background = "#fdf7fa",
        BackgroundGray = "#f5f0f3",
        Surface = "#ffffff",

        AppbarBackground = "#fdf7fa",
        AppbarText = "#4a2a68",

        DrawerBackground = "#fdf7fa",
        DrawerText = "#4a2a68",
        DrawerIcon = "#7a4a9a",

        TextPrimary = "#4a2a68",
        TextSecondary = "#7a5a70",
        TextDisabled = "rgba(74, 42, 104, 0.38)",

        ActionDefault = "#7a4a9a",
        ActionDisabled = "rgba(122, 74, 154, 0.26)",
        ActionDisabledBackground = "rgba(122, 74, 154, 0.12)",

        Divider = "#eedde8",
        DividerLight = "#f5ece8",

        TableLines = "#eedde8",
        TableStriped = "rgba(245, 220, 232, 0.3)",
        TableHover = "rgba(232, 160, 176, 0.1)",

        LinesDefault = "#eedde8",
        LinesInputs = "#c8a0c0",

        Success = "#4caf50",
        Warning = "#ff9800",
        Error = "#f44336",
        Info = "#7a4a9a",
    },
    Typography = new Typography
    {
        Default = new DefaultTypography
        {
            FontFamily = new[] { "Inter", "Helvetica Neue", "Arial", "sans-serif" },
            FontSize = "1rem",
            FontWeight = "400",
            LineHeight = "1.6",
        },
        H1 = new H1Typography { FontFamily = new[] { "Fraunces", "Georgia", "serif" }, FontSize = "2.5rem", FontWeight = "600" },
        H2 = new H2Typography { FontFamily = new[] { "Fraunces", "Georgia", "serif" }, FontSize = "2rem", FontWeight = "600" },
        H3 = new H3Typography { FontFamily = new[] { "Fraunces", "Georgia", "serif" }, FontSize = "1.5rem", FontWeight = "600" },
        H4 = new H4Typography { FontFamily = new[] { "Fraunces", "Georgia", "serif" }, FontSize = "1.25rem", FontWeight = "500" },
        H5 = new H5Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "1rem", FontWeight = "500" },
        H6 = new H6Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.875rem", FontWeight = "500" },
        Button = new ButtonTypography { FontFamily = new[] { "Inter", "sans-serif" }, FontWeight = "500", TextTransform = "none" },
    },
    LayoutProperties = new LayoutProperties
    {
        DefaultBorderRadius = "8px",
        DrawerWidthLeft = "260px",
    },
    Shadows = new Shadow(),
    ZIndex = new ZIndex(),
};

/// <summary>
/// Tema Carbon & Mint — carvão escuro + menta fresca.
/// Moderno, arrojado, dark-first.
/// </summary>
public static MudTheme CarbonMint => new()
{
    PaletteDark = new PaletteDark
    {
        Primary = "#4dc9a0",
        PrimaryDarken = "#2a9a78",
        PrimaryLighten = "#80e0c0",
        PrimaryContrastText = "#0a3028",

        Secondary = "#d0f0e4",
        SecondaryDarken = "#a8d8c8",
        SecondaryLighten = "#e8f8f2",
        SecondaryContrastText = "#0a3028",

        Tertiary = "#2a2e35",
        TertiaryContrastText = "#f4f5f6",

        Background = "#15181e",
        BackgroundGray = "#1e2228",
        Surface = "#2a2e35",

        AppbarBackground = "#15181e",
        AppbarText = "#f4f5f6",

        DrawerBackground = "#1e2228",
        DrawerText = "#f4f5f6",
        DrawerIcon = "#4dc9a0",

        TextPrimary = "#f4f5f6",
        TextSecondary = "#9aa0ac",
        TextDisabled = "rgba(244, 245, 246, 0.38)",

        ActionDefault = "#4dc9a0",
        ActionDisabled = "rgba(77, 201, 160, 0.26)",
        ActionDisabledBackground = "rgba(77, 201, 160, 0.12)",

        Divider = "rgba(77, 201, 160, 0.15)",
        DividerLight = "rgba(77, 201, 160, 0.08)",

        TableLines = "rgba(77, 201, 160, 0.15)",
        TableStriped = "rgba(77, 201, 160, 0.05)",
        TableHover = "rgba(77, 201, 160, 0.08)",

        LinesDefault = "rgba(77, 201, 160, 0.2)",
        LinesInputs = "rgba(77, 201, 160, 0.4)",

        Success = "#4dc9a0",
        Warning = "#f0a830",
        Error = "#f06060",
        Info = "#60a8d0",

        OverlayDark = "rgba(15, 18, 24, 0.85)",
        OverlayLight = "rgba(42, 46, 53, 0.5)",
    },
    Typography = new Typography
    {
        Default = new DefaultTypography
        {
            FontFamily = new[] { "Inter", "Helvetica Neue", "Arial", "sans-serif" },
            FontSize = "1rem",
            FontWeight = "400",
            LineHeight = "1.6",
        },
        H1 = new H1Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "2.5rem", FontWeight = "500" },
        H2 = new H2Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "2rem", FontWeight = "500" },
        H3 = new H3Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "1.5rem", FontWeight = "500" },
        H4 = new H4Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "1.25rem", FontWeight = "500" },
        H5 = new H5Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "1rem", FontWeight = "500" },
        H6 = new H6Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.875rem", FontWeight = "500" },
        Button = new ButtonTypography { FontFamily = new[] { "Inter", "sans-serif" }, FontWeight = "500", TextTransform = "none" },
    },
    LayoutProperties = new LayoutProperties
    {
        DefaultBorderRadius = "8px",
        DrawerWidthLeft = "260px",
    },
    Shadows = new Shadow(),
    ZIndex = new ZIndex(),
};
```

---

## Passo 2 — Adicionar variáveis CSS em `app.css`

**Arquivo:** `HomePlanner.BlazorServer/wwwroot/css/app.css`

**Instrução para o Claude:**

> O arquivo `app.css` já tem uma seção com variáveis CSS (`:root { ... }`) para o tema Sage/Terracotta.
> Adiciona as duas novas seções abaixo **depois** da seção `:root` existente, sem alterar nada do que já está.

```css
/* =============================================
   TEMA: Plum & Blush
   Ativar adicionando class="theme-plum" no <body>
   ============================================= */
body.theme-plum {
    --hp-primary:          #7a4a9a;
    --hp-primary-dark:     #4a2a68;
    --hp-primary-light:    #c8a0e0;
    --hp-accent:           #e8a0b0;
    --hp-accent-light:     #f5dce8;
    --hp-bg:               #fdf7fa;
    --hp-bg-secondary:     #f5f0f3;
    --hp-surface:          #ffffff;
    --hp-text-primary:     #4a2a68;
    --hp-text-secondary:   #7a5a70;
    --hp-text-muted:       #a08898;
    --hp-border:           #eedde8;
    --hp-border-strong:    #c8a0c0;
    --hp-shadow:           rgba(122, 74, 154, 0.08);

    /* Nav / Appbar */
    --hp-nav-bg:           #fdf7fa;
    --hp-nav-text:         #4a2a68;
    --hp-nav-border:       #eedde8;

    /* Landing hero gradient */
    --hp-hero-from:        #7a4a9a;
    --hp-hero-to:          #4a2a68;
    --hp-hero-text:        #fdf7fa;
    --hp-hero-accent:      #e8a0b0;
}

/* =============================================
   TEMA: Carbon & Mint
   Ativar adicionando class="theme-carbon" no <body>
   ============================================= */
body.theme-carbon {
    --hp-primary:          #4dc9a0;
    --hp-primary-dark:     #2a9a78;
    --hp-primary-light:    #80e0c0;
    --hp-accent:           #d0f0e4;
    --hp-accent-light:     #e8f8f2;
    --hp-bg:               #15181e;
    --hp-bg-secondary:     #1e2228;
    --hp-surface:          #2a2e35;
    --hp-text-primary:     #f4f5f6;
    --hp-text-secondary:   #9aa0ac;
    --hp-text-muted:       #6a7080;
    --hp-border:           rgba(77, 201, 160, 0.15);
    --hp-border-strong:    rgba(77, 201, 160, 0.35);
    --hp-shadow:           rgba(0, 0, 0, 0.4);

    /* Nav / Appbar */
    --hp-nav-bg:           #15181e;
    --hp-nav-text:         #f4f5f6;
    --hp-nav-border:       rgba(77, 201, 160, 0.12);

    /* Landing hero */
    --hp-hero-from:        #15181e;
    --hp-hero-to:          #2a2e35;
    --hp-hero-text:        #f4f5f6;
    --hp-hero-accent:      #4dc9a0;
}
```

---

## Passo 3 — Adicionar seletor de tema no `MainLayout.razor`

**Arquivo:** `HomePlanner.BlazorServer/Components/Layout/MainLayout.razor`

**Instrução para o Claude:**

> No `MainLayout.razor`, o `MudThemeProvider` já está usando `HomePlannerTheme.Light`.
> Substitui a implementação para suportar troca dinâmica de tema entre os três disponíveis.
> O tema ativo deve ser persistido no `localStorage` do browser entre sessões.

```razor
@inherits LayoutComponentBase
@inject IJSRuntime JS

<MudThemeProvider @ref="_mudThemeProvider" Theme="_temaAtivo" IsDarkMode="_isDark" />
<MudDialogProvider />
<MudSnackbarProvider />

<MudLayout>
    <MudAppBar Elevation="0" Dense="true" Style="border-bottom: 1px solid var(--hp-nav-border);">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" Color="Color.Inherit" Edge="Edge.Start"
                       OnClick="@ToggleDrawer" />
        <MudText Typo="Typo.H6" Style="font-weight: 500;">HomePlanner</MudText>
        <MudSpacer />

        @* Seletor de tema *@
        <MudMenu Icon="@Icons.Material.Outlined.Palette" Color="Color.Inherit" Dense="true">
            <MudMenuItem OnClick="@(() => AplicarTema("sage"))">
                <MudStack Row AlignItems="AlignItems.Center" Spacing="2">
                    <div style="width:14px;height:14px;border-radius:50%;background:#5a8a6a;"></div>
                    <span>Sage &amp; Terracotta</span>
                    @if (_temaNome == "sage") { <MudIcon Icon="@Icons.Material.Filled.Check" Size="Size.Small" /> }
                </MudStack>
            </MudMenuItem>
            <MudMenuItem OnClick="@(() => AplicarTema("plum"))">
                <MudStack Row AlignItems="AlignItems.Center" Spacing="2">
                    <div style="width:14px;height:14px;border-radius:50%;background:#7a4a9a;"></div>
                    <span>Plum &amp; Blush</span>
                    @if (_temaNome == "plum") { <MudIcon Icon="@Icons.Material.Filled.Check" Size="Size.Small" /> }
                </MudStack>
            </MudMenuItem>
            <MudMenuItem OnClick="@(() => AplicarTema("carbon"))">
                <MudStack Row AlignItems="AlignItems.Center" Spacing="2">
                    <div style="width:14px;height:14px;border-radius:50%;background:#4dc9a0;"></div>
                    <span>Carbon &amp; Mint</span>
                    @if (_temaNome == "carbon") { <MudIcon Icon="@Icons.Material.Filled.Check" Size="Size.Small" /> }
                </MudStack>
            </MudMenuItem>
        </MudMenu>
    </MudAppBar>

    <MudDrawer @bind-Open="_drawerOpen" Elevation="0">
        <NavMenu />
    </MudDrawer>

    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private MudThemeProvider _mudThemeProvider = null!;
    private bool _drawerOpen = true;
    private bool _isDark = false;
    private string _temaNome = "sage";
    private MudTheme _temaAtivo = HomePlannerTheme.Light;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var temaSalvo = await JS.InvokeAsync<string?>("localStorage.getItem", "hp-theme");
            if (!string.IsNullOrEmpty(temaSalvo))
                await AplicarTema(temaSalvo);
        }
    }

    private async Task AplicarTema(string nome)
    {
        _temaNome = nome;
        (_temaAtivo, _isDark) = nome switch
        {
            "plum"   => (HomePlannerTheme.PlumBlush,  false),
            "carbon" => (HomePlannerTheme.CarbonMint, true),
            _        => (HomePlannerTheme.Light,       false),
        };

        // Aplica class CSS no body para variáveis CSS customizadas
        var cssClass = nome switch
        {
            "plum"   => "theme-plum",
            "carbon" => "theme-carbon",
            _        => "",
        };
        await JS.InvokeVoidAsync("eval",
            $"document.body.className = document.body.className.replace(/theme-\\S+/g, '').trim(); " +
            $"if ('{cssClass}') document.body.classList.add('{cssClass}');");

        await JS.InvokeVoidAsync("localStorage.setItem", "hp-theme", nome);
        StateHasChanged();
    }

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;
}
```

---

## Passo 4 — Aplicar tema na landing page (PublicLayout)

**Arquivo:** `HomePlanner.BlazorServer/Components/Layout/PublicLayout.razor`

**Instrução para o Claude:**

> O `PublicLayout` não precisa do seletor de temas — a landing usa sempre o tema padrão (Sage).
> Mas garante que o `MudThemeProvider` está presente:

```razor
@inherits LayoutComponentBase

<MudThemeProvider Theme="HomePlannerTheme.Light" />
<MudDialogProvider />
<MudSnackbarProvider />

@Body
```

---

## Resumo dos arquivos a modificar

| Arquivo | Ação |
|---------|------|
| `Theme/HomePlannerTheme.cs` | Adicionar propriedades `PlumBlush` e `CarbonMint` |
| `wwwroot/css/app.css` | Adicionar seções `body.theme-plum` e `body.theme-carbon` |
| `Components/Layout/MainLayout.razor` | Substituir para suportar troca dinâmica de tema |
| `Components/Layout/PublicLayout.razor` | Confirmar que tem `MudThemeProvider` |

---

## Prompt pronto para colar no Claude (VS Code)

```
Vou te passar instruções para adicionar dois novos temas visuais ao projeto HomePlanner.
O projeto usa .NET 8, Blazor Server e MudBlazor.

O tema atual (Sage/Terracotta) já está implementado em:
- HomePlanner.BlazorServer/Theme/HomePlannerTheme.cs
- wwwroot/css/app.css
- Components/Layout/MainLayout.razor

Faça as seguintes alterações:

1. Em `HomePlannerTheme.cs`: adiciona duas novas propriedades estáticas `PlumBlush` e `CarbonMint`
   com as paletas e tipografias definidas abaixo. Não altera a propriedade `Light` existente.

2. Em `app.css`: adiciona as seções `body.theme-plum` e `body.theme-carbon` com as variáveis CSS
   definidas abaixo, após a seção `:root` existente.

3. Em `MainLayout.razor`: substitui a implementação para suportar troca dinâmica entre os três temas
   via menu de paleta no AppBar, persistindo a escolha no localStorage.

4. Em `PublicLayout.razor`: confirma que `MudThemeProvider` está presente com `HomePlannerTheme.Light`.

[COLAR AQUI O CONTEÚDO COMPLETO DO ARQUIVO THEME_INSTRUCTIONS.md]
```

---

*Gerado em: 2026-06-05 | Versão: 1.0 | Paletas: Plum & Blush, Carbon & Mint*
