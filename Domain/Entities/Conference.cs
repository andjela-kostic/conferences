namespace Domain.Entities;

public class Conference
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}