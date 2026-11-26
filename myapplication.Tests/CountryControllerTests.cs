using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapplication.Controllers;
using myapplication.Data;
using myapplication.Models;
using NUnit.Framework;

namespace myapplication.Tests;

[TestFixture]
public class CountryControllerTests
{
    [Test]
    public async Task GetAll_ReturnsCountriesOrderedByName()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Countries.AddRange(
            new Country { Name = "India", Iso2Code = "IN", Iso3Code = "IND" },
            new Country { Name = "Australia", Iso2Code = "AU", Iso3Code = "AUS" });
        await dbContext.SaveChangesAsync();

        var controller = new CountryController(dbContext, Microsoft.Extensions.Logging.Abstractions.NullLogger<CountryController>.Instance);

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var countries = okResult!.Value as List<Country>;
        Assert.That(countries, Is.Not.Null);
        Assert.That(countries, Has.Count.EqualTo(2));
        Assert.That(countries![0].Name, Is.EqualTo("Australia"));
        Assert.That(countries[1].Name, Is.EqualTo("India"));
    }

    [Test]
    public async Task GetAll_ReturnsEmptyList_WhenNoCountriesExist()
    {
        await using var dbContext = CreateDbContext();
        var controller = new CountryController(dbContext, Microsoft.Extensions.Logging.Abstractions.NullLogger<CountryController>.Instance);

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var countries = okResult!.Value as List<Country>;
        Assert.That(countries, Is.Not.Null);
        Assert.That(countries, Is.Empty);
    }


    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
