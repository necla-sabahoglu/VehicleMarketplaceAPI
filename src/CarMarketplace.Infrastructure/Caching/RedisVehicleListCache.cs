using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.DTOs.Common;
using CarMarketplace.Application.DTOs.Vehicles;
using StackExchange.Redis;

namespace CarMarketplace.Infrastructure.Caching;

public class RedisVehicleListCache : IVehicleListCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IConnectionMultiplexer _redis;
    private const string VersionKey = "vehicle:list:version";

    public RedisVehicleListCache(IConnectionMultiplexer redis) => _redis = redis;

    public async Task BumpListVersionAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StringIncrementAsync(VersionKey);
    }

    public async Task<PagedResult<VehicleResponse>> GetOrCreateAsync(
        VehicleListQuery query,
        Func<Task<PagedResult<VehicleResponse>>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var verValue = await db.StringGetAsync(VersionKey).ConfigureAwait(false);
        var version = verValue.IsNullOrEmpty ? 0L : (long)verValue;
        var fingerprint = BuildQueryFingerprint(query);
        var key = $"vehicle:list:v{version}:{fingerprint}";

        var cached = await db.StringGetAsync(key).ConfigureAwait(false);
        if (cached.HasValue)
        {
            var parsed = JsonSerializer.Deserialize<PagedResult<VehicleResponse>>(cached!, JsonOptions);
            if (parsed is not null)
                return parsed;
        }

        var result = await factory().ConfigureAwait(false);
        await db.StringSetAsync(key, JsonSerializer.Serialize(result, JsonOptions), ttl).ConfigureAwait(false);
        return result;
    }

    private static string BuildQueryFingerprint(VehicleListQuery q)
    {
        var payload = new
        {
            q.Page,
            q.PageSize,
            q.MinPrice,
            q.MaxPrice,
            Brand = q.Brand?.Trim().ToLowerInvariant(),
            Model = q.Model?.Trim().ToLowerInvariant(),
            q.IncludeInactive
        };
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }
}
