using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myapplication.Services;

namespace myapplication.Controllers;

/// <summary>
/// Token endpoint for development and testing only.
/// Issues bearer tokens for Swagger testing without user interaction.
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous]
[Tags("Token")]
public class TokenController(
    IAzureTokenService tokenService,
    ILogger<TokenController> logger) : ControllerBase
{
    /// <summary>
    /// Get a bearer token for API testing.
    /// Uses DefaultAzureCredential to authenticate with Azure.
    /// Token can be used in Swagger Authorize dialog.
    /// </summary>
    /// <remarks>
    /// This endpoint is for development and testing only.
    /// In production, remove this endpoint or restrict access.
    /// 
    /// DefaultAzureCredential will attempt authentication in this order:
    /// 1. Environment variables
    /// 2. Workload identity (if running in Azure Container Instances/AKS)
    /// 3. Managed identity (if running in Azure App Service)
    /// 4. Visual Studio credentials
    /// 5. Azure CLI credentials (requires 'az login')
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Object containing the access token and expiration time.</returns>
    /// <response code="200">Token acquired successfully.</response>
    /// <response code="401">Failed to authenticate with Azure.</response>
    [HttpGet("get-token")]
    public async Task<ActionResult<TokenResponse>> GetToken(CancellationToken cancellationToken)
    {
        logger.LogInformation("Token request received. Attempting to acquire access token.");

        try
        {
            var token = await tokenService.GetAccessTokenAsync(cancellationToken);

            var response = new TokenResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                Message = "Token acquired successfully. Copy the accessToken value into Swagger Authorize dialog."
            };

            logger.LogInformation("Token issued successfully.");

            return Ok(response);
        }
        catch (Azure.Identity.CredentialUnavailableException exception)
        {
            logger.LogError(exception, "No credential available. Ensure you are logged in with 'az login' or have configured DefaultAzureCredential.");
            return Unauthorized(new { error = "No credential available", details = exception.Message });
        }
        catch (Azure.Identity.AuthenticationFailedException exception)
        {
            logger.LogError(exception, "Authentication failed. Check tenant ID, client ID, and permissions.");
            return Unauthorized(new { error = "Authentication failed", details = exception.Message });
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Token request was canceled.");
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error while acquiring token.");
            return Unauthorized(new { error = "Failed to acquire token", details = exception.Message });
        }
    }
}

/// <summary>
/// Response model for token endpoint.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// The bearer token. Copy this value to Swagger Authorize dialog without the "Bearer " prefix.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token lifetime in seconds (approximate).
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Usage instructions.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
