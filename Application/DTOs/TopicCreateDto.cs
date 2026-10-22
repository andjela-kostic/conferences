namespace Application.DTOs;

/// <summary>Data for creating or updating a topic.</summary>
public class TopicCreateDto
{
    /// <summary>Topic title.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description of the topic.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Identifier of the (existing) conference the topic belongs to.</summary>
    public int ConferenceId { get; set; }
}
