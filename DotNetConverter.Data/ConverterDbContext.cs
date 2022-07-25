using Microsoft.EntityFrameworkCore;

namespace DotNetConverter.Data;

public class ConverterDbContext : DbContext
{
    // ...
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ...
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ...
    }
}