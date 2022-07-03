using YoutubeExplode.Videos.Streams;

namespace DotNetConverter.Models;

public class WorkItem
{
    public StreamManifest? StreamManifest { get; set; }
    public string CallbackUrl { get; set; } = default!;
}