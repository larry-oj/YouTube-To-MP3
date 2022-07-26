using System.Text.Json.Serialization;

namespace DotNetConverter.Models;

public class CheckVideoResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("is_failed")] public bool IsFailed { get; set; }
    [JsonPropertyName("is_finished")] public bool IsFinished { get; set; }

    public CheckVideoResponse(string id, bool isFailed, bool isFinished)
    {
        Id = id;
        IsFailed = isFailed;
        IsFinished = isFinished;
    }
}