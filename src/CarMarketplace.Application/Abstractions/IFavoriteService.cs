using CarMarketplace.Application.DTOs.Vehicles;

namespace CarMarketplace.Application.Abstractions;

public interface IFavoriteService
{
    Task AddAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
}
