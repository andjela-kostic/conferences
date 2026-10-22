using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

/// <summary>A user's registration for a topic.</summary>
public class UserTopicDto
{
    /// <summary>Identifier of the user.</summary>
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    /// <summary>Identifier of the topic.</summary>
    [Range(1, int.MaxValue)]
    public int TopicId { get; set; }
}
