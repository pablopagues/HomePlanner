namespace Domain.HomePlanner.Models.SaaS.Options;

public class AppOptions
{
    public const string SectionName = "App";
    public string Nome { get; set; } = "HomePlanner";
    public string Url { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
}
