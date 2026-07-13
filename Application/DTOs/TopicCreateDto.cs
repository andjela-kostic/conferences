namespace Application.DTOs;

public class TopicCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ConferenceId { get; set; }
}