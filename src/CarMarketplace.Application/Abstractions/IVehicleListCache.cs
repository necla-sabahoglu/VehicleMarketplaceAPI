using CarMarketplace.Application.DTOs.Common;
using CarMarketplace.Application.DTOs.Vehicles;

namespace CarMarketplace.Application.Abstractions;

public interface IVehicleListCache
{
    Task<PagedResult<VehicleResponse>> GetOrCreateAsync(
        VehicleListQuery query,
        Func<Task<PagedResult<VehicleResponse>>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task BumpListVersionAsync(CancellationToken cancellationToken = default);
}
