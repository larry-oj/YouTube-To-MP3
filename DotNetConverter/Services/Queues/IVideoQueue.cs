using DotNetConverter.Models;

namespace DotNetConverter.Services.Queues;

public interface IVideoQueue
{
    Task QueueWorkItemAsync(CancellationToken cancellationToken, string? id, string? url, string? callbackUrl);

    Task<WorkItem> DequeueAsync(CancellationToken cancellationToken);
}