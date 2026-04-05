using CarMarketplace.Domain.Entities;

namespace CarMarketplace.Application.Abstractions;

public interface IFavoriteRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default);
    Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
