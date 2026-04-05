using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.DTOs.Auth;
using CarMarketplace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarMarketplace.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(
        IUserRepository users,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenGenerator jwt)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            PasswordHash = string.Empty
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        var (token, expires) = _jwt.CreateToken(user.Id, user.Email);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Email = user.Email
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null)
            return null;

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            return null;

        var (token, expires) = _jwt.CreateToken(user.Id, user.Email);
        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Email = user.Email
        };
    }
}
