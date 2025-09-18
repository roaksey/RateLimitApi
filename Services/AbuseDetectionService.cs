using RateLimitApi.Data;
using RateLimitApi.Config;

namespace RateLimitApi.Services;

public class AbuseDetectionService
{
    private readonly SqlRepository _repo;
    private readonly AbuseConfig _cfg;

    public AbuseDetectionService(SqlRepository repo, AbuseConfig cfg)
    {
        _repo = repo;
        _cfg = cfg;
    }

    public async Task<bool> CheckAbuse(string apiKey, string ip, string path)
    {
        if (await _repo.CountRequestsInWindow(apiKey, ip, TimeSpan.FromSeconds(_cfg.ShortBurstWindowSeconds)) >
            _cfg.ShortBurstThresholdPerKeyIp)
        {
            await _repo.BlockAsync(apiKey, ip, "Short burst", TimeSpan.FromMinutes(_cfg.ShortBurstBlockMinutes));
            return true;
        }

        if (await _repo.CountRequestsByIp(ip, TimeSpan.FromSeconds(_cfg.ShortBurstWindowSeconds)) >
            _cfg.ShortBurstThresholdPerIp)
        {
            await _repo.BlockAsync(null, ip, "IP short burst", TimeSpan.FromMinutes(_cfg.ShortBurstBlockMinutes));
            return true;
        }

        if (await _repo.DistinctKeysForIp(ip, TimeSpan.FromMinutes(_cfg.RapidKeySwitchWindowMinutes)) >=
            _cfg.RapidKeySwitchDistinctKeys)
        {
            await _repo.BlockAsync(null, ip, "Rapid key switching", TimeSpan.FromMinutes(_cfg.RapidKeySwitchBlockMinutes));
            return true;
        }

        return false;
    }
}
