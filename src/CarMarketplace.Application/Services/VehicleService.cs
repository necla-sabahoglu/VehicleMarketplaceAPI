using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.DTOs.Common;
using CarMarketplace.Application.DTOs.Vehicles;
using CarMarketplace.Application.Events;
using CarMarketplace.Domain.Entities;

namespace CarMarketplace.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicles;
    private readonly IVehicleListCache _listCache;
    private readonly IMessagePublisher _publisher;

    public VehicleService(
        IVehicleRepository vehicles,
        IVehicleListCache listCache,
        IMessagePublisher publisher)
    {
        _vehicles = vehicles;
        _listCache = listCache;
        _publisher = publisher;
    }

    public async Task<VehicleResponse> CreateAsync(Guid sellerId, CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var days = request.ListingDurationDays <= 0 ? 30 : request.ListingDurationDays;
        var now = DateTime.UtcNow;
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            Price = request.Price,
            MileageKm = request.MileageKm,
            Description = request.Description?.Trim(),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(days),
            IsActive = true
        };

        await _vehicles.AddAsync(vehicle, cancellationToken);
        await _vehicles.SaveChangesAsync(cancellationToken);

        await _listCache.BumpListVersionAsync(cancellationToken);

        await _publisher.PublishVehicleCreatedAsync(new VehicleCreatedEvent
        {
            VehicleId = vehicle.Id,
            SellerId = sellerId,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Price = vehicle.Price,
            CreatedAtUtc = vehicle.CreatedAtUtc
        }, cancellationToken);

        return Map(vehicle);
    }

    public Task<PagedResult<VehicleResponse>> ListAsync(VehicleListQuery query, CancellationToken cancellationToken = default)
    {
        var q = new VehicleListQuery
        {
            Page = query.Page < 1 ? 1 : query.Page,
            PageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            Brand = query.Brand,
            Model = query.Model,
            IncludeInactive = query.IncludeInactive
        };

        return _listCache.GetOrCreateAsync(
            q,
            async () =>
            {
                var (items, total) = await _vehicles.SearchAsync(q, cancellationToken);
                return new PagedResult<VehicleResponse>
                {
                    Items = items.Select(Map).ToList(),
                    Page = q.Page,
                    PageSize = q.PageSize,
                    TotalCount = total
                };
            },
            TimeSpan.FromMinutes(2),
            cancellationToken);
    }

    private static VehicleResponse Map(Vehicle v) => new()
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
    };
}
