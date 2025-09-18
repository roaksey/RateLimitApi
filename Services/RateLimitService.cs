using RateLimitApi.Data;

namespace RateLimitApi.Services;

public class RateLimitService
{
    private readonly SqlRepository _repo;
    private readonly AbuseDetectionService _abuse;
    private readonly AlertService _alerts;

    public RateLimitService(SqlRepository repo, AbuseDetectionService abuse, AlertService alerts)
    {
        _repo = repo;
        _abuse = abuse;
        _alerts = alerts;
    }

    public async Task<RequestResult> CheckAndProcessRequest(string apiKey, string ip, string path)
    {
        var plan = await _repo.GetPlanAsync(apiKey);
        if (plan == null)
            return new RequestResult { Allowed = false, Message = "Invalid key" };

        if (await _repo.IsBlockedAsync(apiKey, ip))
            return new RequestResult { Allowed = false, Message = "Blocked" };

        var limit = plan switch
        {
            "Free" => 100,
            "Basic" => 1000,
            "Pro" => int.MaxValue,
            _ => 100
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var current = await _repo.GetUsageAsync(today, apiKey, ip);

        if (limit < int.MaxValue && current * 100 / limit >= 80)
            await _alerts.SendApproachingLimitAlert(plan, apiKey, ip, current, limit);

        if (current >= limit)
        {
            await _repo.LogAsync(apiKey, ip, path, 429, "Daily limit reached");
            return new RequestResult { Allowed = false, Message = "Daily limit reached" };
        }

        await _repo.IncrementUsageAsync(today, apiKey, ip);

        if (await _abuse.CheckAbuse(apiKey, ip, path))
            return new RequestResult { Allowed = false, Message = "Abuse detected" };

        await _repo.LogAsync(apiKey, ip, path, 200, null);
        return new RequestResult { Allowed = true, Message = "OK" };
    }
}
