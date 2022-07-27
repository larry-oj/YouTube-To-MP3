using DotNetConverter.Data;
using DotNetConverter.Data.Models;
using DotNetConverter.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetConverter.Services;

public class TimedCleanupService : IHostedService, IDisposable
{
    private readonly ILogger<TimedCleanupService> _logger;
    private readonly IDbContextFactory<ConverterDbContext> _contextFactory;
    private readonly IConfiguration _configuration;
    private Timer _timer;

    public TimedCleanupService(ILogger<TimedCleanupService> logger, 
        IDbContextFactory<ConverterDbContext> contextFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _configuration = configuration;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed cleanup service running");

        _timer = new Timer(Clean, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        
        return Task.CompletedTask;
    }

    private void Clean(object? state)
    {
        using (var repo = new Repo<QueuedItem>(_contextFactory))
        {
            var items = repo.GetAll().Where(i => i.IsFinished == true || i.IsFailed == true);
            foreach (var item in items)
            {
                _logger.LogInformation($"Cleaning {item.Id}");
                if (!int.TryParse(_configuration.GetSection("Converter:MaxAgeMinutes").Value, out var maxAge)) maxAge = 10;
                if (DateTime.UtcNow.Subtract((DateTime)item.TimeFinished!).TotalMinutes < maxAge) continue;
                File.Delete($"{_configuration.GetSection("Converter:TempDir").Value}/{item.Id}.mp3"); // fun fact: linux is sensitive to / and \, somewhy
                repo.Delete(item);
            }
            repo.Save();
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}