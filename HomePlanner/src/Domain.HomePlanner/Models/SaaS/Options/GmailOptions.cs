namespace Domain.HomePlanner.Models.SaaS.Options;

public class GmailOptions
{
    public const string SectionName = "Gmail";
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NomeRemetente { get; set; } = "HomePlanner";
}
