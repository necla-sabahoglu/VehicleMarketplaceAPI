using CarMarketplace.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CarMarketplace.Infrastructure.Jobs;

public class ListingExpirationJob : IListingExpirationJob
{
    private readonly IVehicleRepository _vehicles;
    private readonly IVehicleListCache _listCache;
    private readonly ILogger<ListingExpirationJob> _logger;

    public ListingExpirationJob(
        IVehicleRepository vehicles,
        IVehicleListCache listCache,
        ILogger<ListingExpirationJob> logger)
    {
        _vehicles = vehicles;
        _listCache = listCache;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var updated = await _vehicles.DeactivateExpiredAsync(now, cancellationToken).ConfigureAwait(false);
        if (updated > 0)
        {
            await _listCache.BumpListVersionAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Deactivated {Count} expired vehicle listings.", updated);
        }
    }
}
