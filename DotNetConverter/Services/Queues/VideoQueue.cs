using System.Collections.Concurrent;
using DotNetConverter.Data;
using DotNetConverter.Data.Models;
using DotNetConverter.Data.Repositories;
using DotNetConverter.Models;
using Microsoft.EntityFrameworkCore;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace DotNetConverter.Services.Queues;

public class VideoQueue : IVideoQueue
{
    private readonly ConcurrentQueue<WorkItem> _workItems;
    private readonly SemaphoreSlim _signal;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VideoQueue> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IDbContextFactory<ConverterDbContext> _contextFactory;

    public VideoQueue(IConfiguration configuration,
        ILogger<VideoQueue> logger, 
        IHttpClientFactory clientFactory,
        IDbContextFactory<ConverterDbContext> contextFactory)
    {
        this._configuration = configuration;
        this._logger = logger;
        _clientFactory = clientFactory;
        this._workItems = new ConcurrentQueue<WorkItem>();
        this._signal = new SemaphoreSlim(0);
        this._contextFactory = contextFactory;
    }

    public async Task<string> QueueWorkItemAsync(CancellationToken cancellationToken, string? url, bool withCallback,
        string? callbackUrl)
    {
        _logger.LogInformation("Verifying data");

        if (url is null or "")
        {
            _logger.LogError("Url is null");
            throw new ArgumentNullException("Url is null");
        }

        var ytClient = new YoutubeClient(_clientFactory.CreateClient());
        
        _logger.LogInformation("Verifying video");
        
        var video = await ytClient.Videos.GetAsync(url, cancellationToken);
        if (video.Duration!.Value.TotalMinutes > 10)
        {
            _logger.LogError("Video can't be longer than 10 minutes");
            throw new ArgumentException("Video can't be longer than 10 minutes");
        }

        StreamManifest? streamManifest = null;
        try
        {
            streamManifest = await ytClient.Videos.Streams.GetManifestAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting stream manifest");
            throw new ArgumentException("Sorry! This track is restricted (likely cause: age restriction)");
        }
        
        if (streamManifest == null)
        {
            _logger.LogError("Video is unavailable");
            throw new ArgumentException("Video is unavailable");
        }
        
        if (cancellationToken.IsCancellationRequested) return "";
        
        _logger.LogInformation("Queueing work item");
        string id;
        using (var repo = new Repo<QueuedItem>(_contextFactory))
        {
            do
            {
                id = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10)
                    .Select(s => s[new Random().Next(s.Length)]).ToArray());
            } while (repo.Get(id) != null);
            
            var workItem = new WorkItem {
                Id = id,
                Name = video.Title,
                StreamManifest = streamManifest,
                WithCallback = withCallback,
                CallbackUrl = callbackUrl
            };

            _workItems.Enqueue(workItem);
            
            repo.Insert(new QueuedItem(id, video.Title));
            await repo.SaveAsync();
        }
        
        // Release the semaphore so that the background thread can start.
        _signal.Release();
        _logger.LogInformation("Queue: work item added");
        return id;
    }

    public async Task<WorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);  
        _workItems.TryDequeue(out var workItem);  
  
        return workItem!;  
    }
}