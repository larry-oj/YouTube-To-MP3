using YoutubeExplode.Videos.Streams;

namespace DotNetConverter.Models;

public class WorkItem
{
    public string Id { get; set; } = default!;
    public StreamManifest? StreamManifest { get; set; }
    public bool WithCallback { get; set; }
    public string CallbackUrl { get; set; } = default!;
}