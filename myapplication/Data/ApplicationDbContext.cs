using Microsoft.EntityFrameworkCore;
using myapplication.Models;

namespace myapplication.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Country");
            entity.HasKey(country => country.CountryId);
            entity.Property(country => country.Name).HasMaxLength(100).IsRequired();
            entity.Property(country => country.Iso2Code).HasMaxLength(2).IsRequired();
            entity.Property(country => country.Iso3Code).HasMaxLength(3).IsRequired();
            entity.HasIndex(country => country.Name).IsUnique();
            entity.HasIndex(country => country.Iso2Code).IsUnique();
            entity.HasIndex(country => country.Iso3Code).IsUnique();
        });
    }
}
