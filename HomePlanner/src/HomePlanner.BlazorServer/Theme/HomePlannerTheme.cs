using MudBlazor;

namespace HomePlanner.BlazorServer.Theme;

public static class HomePlannerTheme
{
    public static readonly MudTheme Tema = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#4A6B5C",
            PrimaryContrastText = "#FAF7F2",
            Secondary = "#C97B5A",
            SecondaryContrastText = "#FAF7F2",
            Background = "#FAF7F2",
            Surface = "#FAF7F2",
            BackgroundGray = "#E8DDD1",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#2A3A33",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#2A3A33",
            TextPrimary = "#2A3A33",
            TextSecondary = "#7A8278",
            ActionDefault = "#4A6B5C",
            Divider = "#E8DDD1",
            Success = "#5B8A6A",
            Warning = "#D4956A",
            Error = "#B05050",
            Info = "#6A8FB0",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "260px",
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["Inter", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = 400,
                LineHeight = 1.5,
            },
            H1 = new H1 { FontFamily = ["Fraunces", "serif"], FontSize = "2.5rem",  FontWeight = 300 },
            H2 = new H2 { FontFamily = ["Fraunces", "serif"], FontSize = "2rem",    FontWeight = 300 },
            H3 = new H3 { FontFamily = ["Fraunces", "serif"], FontSize = "1.75rem", FontWeight = 400 },
            H4 = new H4 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.5rem",  FontWeight = 600 },
            H5 = new H5 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.25rem", FontWeight = 600 },
            H6 = new H6 { FontFamily = ["Inter", "sans-serif"], FontSize = "1rem",    FontWeight = 600 },
            Button = new MudBlazor.Button { FontFamily = ["Inter", "sans-serif"], FontWeight = 500, TextTransform = "none" },
        },
    };

    /// <summary>
    /// Tema Plum &amp; Blush — ameixa escura + rosa quartzo.
    /// Sofisticado, feminino, editorial.
    /// </summary>
    public static readonly MudTheme PlumBlush = new()
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
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "260px",
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["Inter", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = 400,
                LineHeight = 1.6,
            },
            H1 = new H1 { FontFamily = ["Fraunces", "serif"], FontSize = "2.5rem",  FontWeight = 600 },
            H2 = new H2 { FontFamily = ["Fraunces", "serif"], FontSize = "2rem",    FontWeight = 600 },
            H3 = new H3 { FontFamily = ["Fraunces", "serif"], FontSize = "1.5rem",  FontWeight = 600 },
            H4 = new H4 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.25rem", FontWeight = 500 },
            H5 = new H5 { FontFamily = ["Inter", "sans-serif"], FontSize = "1rem",     FontWeight = 500 },
            H6 = new H6 { FontFamily = ["Inter", "sans-serif"], FontSize = "0.875rem", FontWeight = 500 },
            Button = new MudBlazor.Button { FontFamily = ["Inter", "sans-serif"], FontWeight = 500, TextTransform = "none" },
        },
    };

    /// <summary>
    /// Tema Carbon &amp; Mint — carvão escuro + menta fresca.
    /// Moderno, arrojado, dark-first.
    /// </summary>
    public static readonly MudTheme CarbonMint = new()
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
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "260px",
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["Inter", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = 400,
                LineHeight = 1.6,
            },
            H1 = new H1 { FontFamily = ["Inter", "sans-serif"], FontSize = "2.5rem",  FontWeight = 500 },
            H2 = new H2 { FontFamily = ["Inter", "sans-serif"], FontSize = "2rem",    FontWeight = 500 },
            H3 = new H3 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.5rem",  FontWeight = 500 },
            H4 = new H4 { FontFamily = ["Inter", "sans-serif"], FontSize = "1.25rem", FontWeight = 500 },
            H5 = new H5 { FontFamily = ["Inter", "sans-serif"], FontSize = "1rem",     FontWeight = 500 },
            H6 = new H6 { FontFamily = ["Inter", "sans-serif"], FontSize = "0.875rem", FontWeight = 500 },
            Button = new MudBlazor.Button { FontFamily = ["Inter", "sans-serif"], FontWeight = 500, TextTransform = "none" },
        },
    };
}
