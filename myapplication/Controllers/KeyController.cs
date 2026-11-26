using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;

namespace myapplication.Controllers;

[ApiController]
[Route("[controller]")]
public class KeyController(
    SecretClient secretClient,
    ILogger<KeyController> logger) : ControllerBase
{
    [HttpGet("{secretName}")]
    public async Task<ActionResult<object>> GetSecret(
        string secretName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(secretName) ||
            secretName.Length > 127 ||
            secretName.Any(character => !char.IsLetterOrDigit(character) && character != '-'))
        {
            logger.LogWarning("A Key Vault request contained an invalid secret name.");
            return BadRequest("Secret names may contain only letters, numbers, and hyphens.");
        }

        logger.LogInformation("Reading secret {SecretName} from Azure Key Vault.", secretName);

        try
        {
            KeyVaultSecret secret = await secretClient.GetSecretAsync(
                secretName,
                cancellationToken: cancellationToken);

            logger.LogInformation("Successfully read secret {SecretName} from Azure Key Vault.", secretName);

            return Ok(new
            {
                Name = secret.Name,
                Value = secret.Value
            });
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            logger.LogWarning("Secret {SecretName} was not found in Azure Key Vault.", secretName);
            return NotFound(new { Message = $"Secret '{secretName}' was not found." });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("The Key Vault request for secret {SecretName} was canceled.", secretName);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while reading secret {SecretName} from Azure Key Vault.", secretName);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to read the requested secret.");
        }
    }
}