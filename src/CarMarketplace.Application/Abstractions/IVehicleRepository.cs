using CarMarketplace.Application.DTOs.Vehicles;
using CarMarketplace.Domain.Entities;

namespace CarMarketplace.Application.Abstractions;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Vehicle> Items, int TotalCount)> SearchAsync(
        VehicleListQuery query,
        CancellationToken cancellationToken = default);
    Task<int> DeactivateExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
