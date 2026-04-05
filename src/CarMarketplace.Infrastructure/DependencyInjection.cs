using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.Services;
using CarMarketplace.Infrastructure.Auth;
using CarMarketplace.Infrastructure.Caching;
using CarMarketplace.Infrastructure.Jobs;
using CarMarketplace.Infrastructure.Messaging;
using CarMarketplace.Infrastructure.Persistence;
using CarMarketplace.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using CarMarketplace.Domain.Entities;

namespace CarMarketplace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlConnection = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(sqlConnection));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("ConnectionStrings:Redis is required.");
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<IVehicleListCache, RedisVehicleListCache>();

        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddHostedService<VehicleCreatedConsumerHostedService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IListingExpirationJob, ListingExpirationJob>();

        return services;
    }
}
