using System.ComponentModel.DataAnnotations;

namespace DotNetConverter.Data.Models;

public class QueuedItem : IStringIdEntity
{
    [Key] public string Id { get; set; }
    [Required] public string Name { get; set; }
    public bool IsFinished { get; set; } = false;
    public DateTime? TimeFinished { get; set; }
    public bool IsFailed { get; set; } = false;

    public QueuedItem(string id, string name)
    {
        Id = id;
        Name = name;
    }
}