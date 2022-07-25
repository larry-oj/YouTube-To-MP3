using DotNetConverter.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetConverter.Data;

public class ConverterDbContext : DbContext
{
    public DbSet<QueuedItem> QueuedItems { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ...
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ...
    }
}