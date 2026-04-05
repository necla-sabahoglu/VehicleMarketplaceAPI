using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.DTOs.Vehicles;
using CarMarketplace.Domain.Entities;
using CarMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarMarketplace.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _db;

    public VehicleRepository(AppDbContext db) => _db = db;

    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default) =>
        await _db.Vehicles.AddAsync(vehicle, cancellationToken);

    public async Task<(IReadOnlyList<Vehicle> Items, int TotalCount)> SearchAsync(
        VehicleListQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        var q = _db.Vehicles.AsNoTracking().AsQueryable();
        if (!query.IncludeInactive)
            q = q.Where(v => v.IsActive);

        if (query.MinPrice.HasValue)
            q = q.Where(v => v.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue)
            q = q.Where(v => v.Price <= query.MaxPrice.Value);
        if (!string.IsNullOrWhiteSpace(query.Brand))
            q = q.Where(v => v.Brand.Contains(query.Brand.Trim()));
        if (!string.IsNullOrWhiteSpace(query.Model))
            q = q.Where(v => v.Model.Contains(query.Model.Trim()));

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(v => v.CreatedAtUtc)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<int> DeactivateExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        return await _db.Vehicles
            .Where(v => v.IsActive && v.ExpiresAtUtc <= utcNow)
            .ExecuteUpdateAsync(
                s => s.SetProperty(v => v.IsActive, false),
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
