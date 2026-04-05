using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.DTOs.Vehicles;
using CarMarketplace.Domain.Entities;

namespace CarMarketplace.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favorites;
    private readonly IVehicleRepository _vehicles;

    public FavoriteService(IFavoriteRepository favorites, IVehicleRepository vehicles)
    {
        _favorites = favorites;
        _vehicles = vehicles;
    }

    public async Task AddAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicles.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle is null || !vehicle.IsActive)
            throw new KeyNotFoundException("Vehicle not found.");

        if (await _favorites.ExistsAsync(userId, vehicleId, cancellationToken))
            return;

        await _favorites.AddAsync(new Favorite
        {
            UserId = userId,
            VehicleId = vehicleId,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
        await _favorites.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var vehicles = await _favorites.ListByUserAsync(userId, cancellationToken);
        return vehicles.Select(v => new VehicleResponse
        {
            Id = v.Id,
            SellerId = v.SellerId,
            Brand = v.Brand,
            Model = v.Model,
            Year = v.Year,
            Price = v.Price,
            MileageKm = v.MileageKm,
            Description = v.Description,
            CreatedAtUtc = v.CreatedAtUtc,
            ExpiresAtUtc = v.ExpiresAtUtc,
            IsActive = v.IsActive
        }).ToList();
    }
}
