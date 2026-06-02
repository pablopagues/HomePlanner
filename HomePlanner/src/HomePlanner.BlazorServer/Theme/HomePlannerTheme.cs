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
}
