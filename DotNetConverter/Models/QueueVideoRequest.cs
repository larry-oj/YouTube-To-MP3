using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNetConverter.Models;

public class QueueVideoRequest
{
    [Required]
    [JsonPropertyName("id")] 
    public string Id { get; set; }
    
    [Required]
    [JsonPropertyName("url")] 
    public string Url { get; set; }
    
    [Required]
    [JsonPropertyName("callback_url")] 
    public string CallbackUrl { get; set; }
}