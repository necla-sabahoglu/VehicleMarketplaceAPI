namespace CarMarketplace.Application.Events;

public class VehicleCreatedEvent
{
    public Guid VehicleId { get; set; }
    public Guid SellerId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
