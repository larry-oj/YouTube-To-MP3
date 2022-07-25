using System.ComponentModel.DataAnnotations;

namespace DotNetConverter.Data.Models;

public class QueuedItem : IStringIdEntity
{
    [Key] public string Id { get; set; }
    public bool IsFinished { get; set; } = false;
    public DateTime? TimeFinished { get; set; }

    public QueuedItem(string id)
    {
        Id = id;
    }
}