using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapplication.Data;
using myapplication.Models;

namespace myapplication.Controllers;

[ApiController]
[Route("[controller]")]
public class CountryController(
    ApplicationDbContext dbContext,
    ILogger<CountryController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Country>>> GetAll(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving countries from the database.");

        try
        {
            var countries = await dbContext.Countries
                .AsNoTracking()
                .OrderBy(country => country.Name)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {CountryCount} countries from the database.", countries.Count);

            return Ok(countries);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("The country request was canceled by the client.");
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while retrieving countries from the database.");
            throw;
        }
    }
}
