namespace RateLimitApi.Config;

public class AbuseConfig
{
    public int ShortBurstWindowSeconds { get; }
    public int ShortBurstThresholdPerKeyIp { get; }
    public int ShortBurstThresholdPerIp { get; }
    public int ShortBurstBlockMinutes { get; }

    public int RapidKeySwitchWindowMinutes { get; }
    public int RapidKeySwitchDistinctKeys { get; }
    public int RapidKeySwitchBlockMinutes { get; }

    public int AutomationLowMedianMs { get; }
    public int AutomationSampleSize { get; }
    public int AutomationBlockMinutes { get; }

    public AbuseConfig(IConfiguration cfg)
    {
        var s = cfg.GetSection("Abuse");
        ShortBurstWindowSeconds = s.GetValue("ShortBurstWindowSeconds", 60);
        ShortBurstThresholdPerKeyIp = s.GetValue("ShortBurstThresholdPerKeyIp", 50);
        ShortBurstThresholdPerIp = s.GetValue("ShortBurstThresholdPerIp", 200);
        ShortBurstBlockMinutes = s.GetValue("ShortBurstBlockMinutes", 15);

        RapidKeySwitchWindowMinutes = s.GetValue("RapidKeySwitchWindowMinutes", 5);
        RapidKeySwitchDistinctKeys = s.GetValue("RapidKeySwitchDistinctKeys", 5);
        RapidKeySwitchBlockMinutes = s.GetValue("RapidKeySwitchBlockMinutes", 60);

        AutomationLowMedianMs = s.GetValue("AutomationLowMedianMs", 200);
        AutomationSampleSize = s.GetValue("AutomationSampleSize", 20);
        AutomationBlockMinutes = s.GetValue("AutomationBlockMinutes", 30);
    }
}
