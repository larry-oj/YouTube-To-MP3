using DotNetConverter.Models;

namespace DotNetConverter.Services.Queues;

public interface IVideoQueue
{
    Task<string> QueueWorkItemAsync(CancellationToken cancellationToken, string? url, bool withCallback,
        string? callbackUrl);

    Task<WorkItem> DequeueAsync(CancellationToken cancellationToken);
}