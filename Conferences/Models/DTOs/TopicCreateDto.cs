namespace Conferences.Models.DTOs;

public class TopicCreateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int ConferenceId { get; set; }
}