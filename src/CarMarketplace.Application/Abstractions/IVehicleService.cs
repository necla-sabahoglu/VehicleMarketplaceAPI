using CarMarketplace.Application.DTOs.Common;
using CarMarketplace.Application.DTOs.Vehicles;

namespace CarMarketplace.Application.Abstractions;

public interface IVehicleService
{
    Task<VehicleResponse> CreateAsync(Guid sellerId, CreateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<VehicleResponse>> ListAsync(VehicleListQuery query, CancellationToken cancellationToken = default);
}
