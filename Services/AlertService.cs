using RateLimitApi.Config;
using System.Net.Mail;

namespace RateLimitApi.Services;

public class AlertService
{
    private readonly AlertConfig _cfg;
    private readonly IHttpClientFactory _http;

    public AlertService(AlertConfig cfg, IHttpClientFactory http)
    {
        _cfg = cfg;
        _http = http;
    }

    public async Task SendApproachingLimitAlert(string plan, string apiKey, string ip, int current, int limit)
    {
        if (!string.IsNullOrWhiteSpace(_cfg.WebhookUrl))
        {
            var client = _http.CreateClient();
            try
            {
                await client.PostAsJsonAsync(_cfg.WebhookUrl, new { plan, apiKey, ip, current, limit });
            }
            catch { }
        }

        if (_cfg.EmailEnable)
        {
            try
            {
                using var smtp = new SmtpClient(_cfg.EmailHost, _cfg.EmailPort)
                {
                    EnableSsl = _cfg.EmailUseSsl,
                    Credentials = new System.Net.NetworkCredential(_cfg.EmailUser, _cfg.EmailPassword)
                };
                var msg = new MailMessage(_cfg.EmailFrom, _cfg.EmailTo)
                {
                    Subject = $"[RateLimit] {plan} key nearing daily limit",
                    Body = $"ApiKey={apiKey}\nIP={ip}\nUsage={current}/{limit}\nTime={DateTime.UtcNow:u}"
                };
                await smtp.SendMailAsync(msg);
            }
            catch { }
        }
    }
}
