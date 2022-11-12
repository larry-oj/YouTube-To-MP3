using DotNetConverter.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetConverter.Data;

public class ConverterDbContext : DbContext
{
    public DbSet<QueuedItem> QueuedItems { get; set; }

    public ConverterDbContext(DbContextOptions<ConverterDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ...
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ...
    }
}