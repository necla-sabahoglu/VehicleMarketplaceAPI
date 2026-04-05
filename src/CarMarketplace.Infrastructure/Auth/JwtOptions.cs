namespace CarMarketplace.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "CarMarketplace";
    public string Audience { get; set; } = "CarMarketplaceClients";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 60;
}
