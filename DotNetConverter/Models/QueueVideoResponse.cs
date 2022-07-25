using System.Text.Json.Serialization;

namespace DotNetConverter.Models;

public class QueueVideoResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }

    public QueueVideoResponse(string id)
    {
        Id = id;
    }
}