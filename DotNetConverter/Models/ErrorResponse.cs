using System.Text.Json.Serialization;

namespace DotNetConverter.Models;

public class ErrorMessage
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("error_message")] public string Message { get; set; }

    public ErrorMessage(string id, string errorMessage)
    {
        Id = id;
        Message = errorMessage;
    }
}