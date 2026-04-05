namespace CarMarketplace.Application.Abstractions;

public interface IListingExpirationJob
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
