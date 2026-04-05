using CarMarketplace.Application.Events;

namespace CarMarketplace.Application.Abstractions;

public interface IMessagePublisher
{
    Task PublishVehicleCreatedAsync(VehicleCreatedEvent evt, CancellationToken cancellationToken = default);
}
