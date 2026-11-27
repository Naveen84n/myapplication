using Azure.Identity;
using Azure.Core;
using myapplication.Configuration;

namespace myapplication.Services;

public interface IAzureTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

public sealed class AzureTokenService : IAzureTokenService
{
    private readonly AzureAdOptions _options;
    private readonly ILogger<AzureTokenService> _logger;

    public AzureTokenService(AzureAdOptions options, ILogger<AzureTokenService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var credential = new DefaultAzureCredential();
            var scope = $"{_options.Audience}/.default";

            _logger.LogInformation("Requesting access token for scope: {Scope}", scope);

            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { scope }),
                cancellationToken);

            _logger.LogInformation("Successfully obtained access token. Expires: {ExpiresOn}", token.ExpiresOn);

            return token.Token;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to obtain access token from Azure. Ensure DefaultAzureCredential is properly configured.");
            throw;
        }
    }
}
