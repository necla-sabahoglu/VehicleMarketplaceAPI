namespace CarMarketplace.Application.DTOs.Vehicles;

public class CreateVehicleRequest
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int? MileageKm { get; set; }
    public string? Description { get; set; }
    /// <summary>Days until listing expires (default 30).</summary>
    public int ListingDurationDays { get; set; } = 30;
}
