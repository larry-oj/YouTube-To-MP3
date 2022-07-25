using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNetConverter.Models;

public class QueueVideoRequest
{
    [Required] [JsonPropertyName("url")] public string Url { get; set; }
    [JsonPropertyName("with_callback")] public bool WithCallback { get; set; }
    [JsonPropertyName("callback_url")] public string CallbackUrl { get; set; }
}