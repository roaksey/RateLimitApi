namespace RateLimitApi.Config;

public class AlertConfig
{
    public string WebhookUrl { get; }
    public int ApproachPercent { get; }
    public bool EmailEnable { get; }
    public string EmailHost { get; }
    public int EmailPort { get; }
    public bool EmailUseSsl { get; }
    public string EmailUser { get; }
    public string EmailPassword { get; }
    public string EmailFrom { get; }
    public string EmailTo { get; }

    public AlertConfig(IConfiguration cfg)
    {
        WebhookUrl = cfg["Alerts:WebhookUrl"] ?? "";
        ApproachPercent = int.TryParse(cfg["Alerts:ApproachPercent"], out var p) ? p : 80;

        EmailEnable = bool.TryParse(cfg["Alerts:Email:Enable"], out var e) && e;
        EmailHost = cfg["Alerts:Email:SmtpHost"] ?? "";
        EmailPort = int.TryParse(cfg["Alerts:Email:SmtpPort"], out var port) ? port : 587;
        EmailUseSsl = bool.TryParse(cfg["Alerts:Email:UseSsl"], out var ssl) && ssl;
        EmailUser = cfg["Alerts:Email:User"] ?? "";
        EmailPassword = cfg["Alerts:Email:Password"] ?? "";
        EmailFrom = cfg["Alerts:Email:From"] ?? "";
        EmailTo = cfg["Alerts:Email:To"] ?? "";
    }
}
