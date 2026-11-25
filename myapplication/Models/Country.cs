namespace myapplication.Models;

public class Country
{
    public int CountryId { get; set; }

    public required string Name { get; set; }

    public required string Iso2Code { get; set; }

    public required string Iso3Code { get; set; }
}
