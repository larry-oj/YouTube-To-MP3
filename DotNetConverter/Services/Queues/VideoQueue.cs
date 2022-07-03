using System.Collections.Concurrent;
using DotNetConverter.Models;
using YoutubeExplode;

namespace DotNetConverter.Services.Queues;

public class VideoQueue : IVideoQueue
{
    private readonly ConcurrentQueue<WorkItem> _workItems;
    private readonly SemaphoreSlim _signal;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VideoQueue> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public VideoQueue(IConfiguration configuration,
        ILogger<VideoQueue> logger, 
        IHttpClientFactory clientFactory)
    {
        this._configuration = configuration;
        this._logger = logger;
        _clientFactory = clientFactory;
        this._workItems = new ConcurrentQueue<WorkItem>();
        this._signal = new SemaphoreSlim(0);
    }

    public async Task QueueWorkItemAsync(CancellationToken cancellationToken, string? id, string? url, string? callbackUrl)
    {
        _logger.LogInformation("Verifying data");

        if (id is null or "")
        {
            _logger.LogError("Id is null");
            throw new ArgumentNullException("Id is null");
        }
        
        if (url is null or "")
        {
            _logger.LogError("Url is null");
            throw new ArgumentNullException("Url is null");
        }

        if (callbackUrl is null or "")
        {
            _logger.LogError("Callback Url is null");
            throw new ArgumentNullException("Callback Url is null");
        }
        
        var ytClient = new YoutubeClient(_clientFactory.CreateClient());
        
        _logger.LogInformation("Verifying video availability");
        var streamManifest = await ytClient.Videos.Streams.GetManifestAsync(url, cancellationToken);
        if (streamManifest == null)
        {
            _logger.LogError("Video is unavailable");
            throw new ArgumentNullException("Video is unavailable");
        }
        
        _logger.LogInformation("Queueing work item");
        if (!cancellationToken.IsCancellationRequested)
        {
            var workItem = new WorkItem {
                Id = id,
                StreamManifest = streamManifest,
                CallbackUrl = callbackUrl
            };

            _workItems.Enqueue(workItem);

            // Release the semaphore so that the background thread can start.
            _signal.Release();
            _logger.LogInformation("Queue: work item added");
        }
    }

    public async Task<WorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);  
        _workItems.TryDequeue(out var workItem);  
  
        return workItem!;  
    }
}