namespace CarMarketplace.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(Guid userId, string email);
}
