using CarMarketplace.Application.Abstractions;
using CarMarketplace.Domain.Entities;
using CarMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarMarketplace.Infrastructure.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly AppDbContext _db;

    public FavoriteRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default) =>
        _db.Favorites.AnyAsync(f => f.UserId == userId && f.VehicleId == vehicleId, cancellationToken);

    public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default) =>
        await _db.Favorites.AddAsync(favorite, cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Include(f => f.Vehicle)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => f.Vehicle)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
