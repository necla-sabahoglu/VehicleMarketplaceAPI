namespace CarMarketplace.Application.DTOs.Vehicles;

public class VehicleResponse
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int? MileageKm { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; }
}
