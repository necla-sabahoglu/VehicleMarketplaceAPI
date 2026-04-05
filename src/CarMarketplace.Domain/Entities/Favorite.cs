namespace CarMarketplace.Domain.Entities;

public class Favorite
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}
