using System.ComponentModel.DataAnnotations;

namespace DotNetConverter.Data.Models;

public class QueuedItem
{
    [Key] public string Id { get; set; }
    public bool IsFinished { get; set; } = false;
    public DateTime? TimeFinished { get; set; }
    public string Data { get; set; }

    public QueuedItem(string id, string data)
    {
        Id = id;
        Data = data;
    }
}