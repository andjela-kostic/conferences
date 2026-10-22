namespace Application.DTOs;

/// <summary>A user's registration for a topic.</summary>
public class UserTopicDto
{
    /// <summary>Identifier of the user.</summary>
    public int UserId { get; set; }

    /// <summary>Identifier of the topic.</summary>
    public int TopicId { get; set; }
}
