namespace Application.DTOs;

public class ConferenceCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<int> TopicIds { get; set; } = new();
}