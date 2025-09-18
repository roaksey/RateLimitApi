using Microsoft.Data.SqlClient;

namespace RateLimitApi.Data;

public class SqlRepository
{
    private readonly string _connStr;
    public SqlRepository(IConfiguration cfg) => _connStr = cfg.GetConnectionString("Sql")!;
    private SqlConnection GetConn() => new SqlConnection(_connStr);

    public async Task<string?> GetPlanAsync(string apiKey)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("SELECT CASE WHEN IsActive=1 THEN Plan ELSE NULL END FROM dbo.ApiKeys WHERE ApiKey=@k", sql);
        c.Parameters.AddWithValue("@k", apiKey);
        return (string?)await c.ExecuteScalarAsync();
    }

    public async Task<bool> IsBlockedAsync(string apiKey, string ip)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand(@"
SELECT TOP 1 1 FROM dbo.vActiveBlocks
WHERE (ApiKey=@k OR ApiKey IS NULL) AND (IP=@ip OR IP IS NULL)", sql);
        c.Parameters.AddWithValue("@k", apiKey);
        c.Parameters.AddWithValue("@ip", ip);
        return (await c.ExecuteScalarAsync()) != null;
    }

    public async Task<int> GetUsageAsync(DateOnly date, string apiKey, string ip)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand(@"
MERGE dbo.DailyUsage AS t
USING (SELECT @d AS UsageDate, @k AS ApiKey, @ip AS IP) AS s
ON (t.UsageDate = s.UsageDate AND t.ApiKey = s.ApiKey AND t.IP = s.IP)
WHEN NOT MATCHED THEN INSERT (UsageDate, ApiKey, IP, Count) VALUES (s.UsageDate, s.ApiKey, s.IP, 0)
OUTPUT COALESCE(INSERTED.Count, DELETED.Count);", sql);
        c.Parameters.AddWithValue("@d", date.ToDateTime(TimeOnly.MinValue));
        c.Parameters.AddWithValue("@k", apiKey);
        c.Parameters.AddWithValue("@ip", ip);
        return Convert.ToInt32(await c.ExecuteScalarAsync());
    }

    public async Task IncrementUsageAsync(DateOnly date, string apiKey, string ip)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("UPDATE dbo.DailyUsage SET Count = Count+1 WHERE UsageDate=@d AND ApiKey=@k AND IP=@ip", sql);
        c.Parameters.AddWithValue("@d", date.ToDateTime(TimeOnly.MinValue));
        c.Parameters.AddWithValue("@k", apiKey);
        c.Parameters.AddWithValue("@ip", ip);
        await c.ExecuteNonQueryAsync();
    }

    public async Task LogAsync(string apiKey, string ip, string path, int status, string? note)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("INSERT INTO dbo.RequestLog (ApiKey, IP, Path, StatusCode, Note) VALUES (@k,@ip,@p,@s,@n)", sql);
        c.Parameters.AddWithValue("@k", apiKey);
        c.Parameters.AddWithValue("@ip", ip);
        c.Parameters.AddWithValue("@p", path);
        c.Parameters.AddWithValue("@s", status);
        c.Parameters.AddWithValue("@n", (object?)note ?? DBNull.Value);
        await c.ExecuteNonQueryAsync();
    }

    public async Task BlockAsync(string? apiKey, string? ip, string reason, TimeSpan duration)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("INSERT INTO dbo.Blocks (ApiKey, IP, Reason, BlockedUntilUtc) VALUES (@k,@ip,@r,DATEADD(SECOND,@sec,SYSUTCDATETIME()))", sql);
        c.Parameters.AddWithValue("@k", (object?)apiKey ?? DBNull.Value);
        c.Parameters.AddWithValue("@ip", (object?)ip ?? DBNull.Value);
        c.Parameters.AddWithValue("@r", reason);
        c.Parameters.AddWithValue("@sec", (int)duration.TotalSeconds);
        await c.ExecuteNonQueryAsync();
    }

    public async Task<int> CountRequestsInWindow(string apiKey, string ip, TimeSpan window)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("SELECT COUNT(*) FROM dbo.RequestLog WHERE ApiKey=@k AND IP=@ip AND AtUtc > DATEADD(SECOND,-@sec,SYSUTCDATETIME())", sql);
        c.Parameters.AddWithValue("@k", apiKey);
        c.Parameters.AddWithValue("@ip", ip);
        c.Parameters.AddWithValue("@sec", (int)window.TotalSeconds);
        return Convert.ToInt32(await c.ExecuteScalarAsync());
    }

    public async Task<int> CountRequestsByIp(string ip, TimeSpan window)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("SELECT COUNT(*) FROM dbo.RequestLog WHERE IP=@ip AND AtUtc > DATEADD(SECOND,-@sec,SYSUTCDATETIME())", sql);
        c.Parameters.AddWithValue("@ip", ip);
        c.Parameters.AddWithValue("@sec", (int)window.TotalSeconds);
        return Convert.ToInt32(await c.ExecuteScalarAsync());
    }

    public async Task<int> DistinctKeysForIp(string ip, TimeSpan window)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("SELECT COUNT(DISTINCT ApiKey) FROM dbo.RequestLog WHERE IP=@ip AND AtUtc > DATEADD(MINUTE,-@min,SYSUTCDATETIME())", sql);
        c.Parameters.AddWithValue("@ip", ip);
        c.Parameters.AddWithValue("@min", (int)window.TotalMinutes);
        return Convert.ToInt32(await c.ExecuteScalarAsync());
    }

    public async Task<List<BlockRecord>> GetActiveBlocksAsync()
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        var c = new SqlCommand("SELECT Id, ApiKey, IP, Reason, BlockedUntilUtc, CreatedAtUtc FROM dbo.vActiveBlocks", sql);
        using var rd = await c.ExecuteReaderAsync();
        var list = new List<BlockRecord>();
        while (await rd.ReadAsync())
        {
            list.Add(new BlockRecord
            {
                Id = rd.GetInt64(0),
                ApiKey = rd.IsDBNull(1) ? null : rd.GetString(1),
                IP = rd.IsDBNull(2) ? null : rd.GetString(2),
                Reason = rd.GetString(3),
                BlockedUntilUtc = rd.GetDateTime(4),
                CreatedAtUtc = rd.GetDateTime(5)
            });
        }
        return list;
    }

    public async Task<int> UnblockAsync(UnblockRequest req)
    {
        using var sql = GetConn();
        await sql.OpenAsync();
        if (req.Id.HasValue)
        {
            var c = new SqlCommand("UPDATE dbo.Blocks SET ClearedAtUtc=SYSUTCDATETIME() WHERE Id=@id", sql);
            c.Parameters.AddWithValue("@id", req.Id.Value);
            return await c.ExecuteNonQueryAsync();
        }
        else
        {
            var c = new SqlCommand("UPDATE dbo.Blocks SET ClearedAtUtc=SYSUTCDATETIME() WHERE ClearedAtUtc IS NULL AND (@k IS NULL OR ApiKey=@k) AND (@ip IS NULL OR IP=@ip)", sql);
            c.Parameters.AddWithValue("@k", (object?)req.ApiKey ?? DBNull.Value);
            c.Parameters.AddWithValue("@ip", (object?)req.IP ?? DBNull.Value);
            return await c.ExecuteNonQueryAsync();
        }
    }
}
