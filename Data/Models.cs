namespace RateLimitApi.Data;

public record UnblockRequest(long? Id, string? ApiKey, string? IP);

public class BlockRecord
{
    public long Id { get; set; }
    public string? ApiKey { get; set; }
    public string? IP { get; set; }
    public string Reason { get; set; } = "";
    public DateTime BlockedUntilUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class RequestResult
{
    public bool Allowed { get; set; }
    public string Message { get; set; } = "";
}
