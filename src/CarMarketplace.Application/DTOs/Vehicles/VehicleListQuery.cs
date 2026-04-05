namespace CarMarketplace.Application.DTOs.Vehicles;

public class VehicleListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public bool IncludeInactive { get; set; }
}
