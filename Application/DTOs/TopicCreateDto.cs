using System.ComponentModel.DataAnnotations;
using Domain;

namespace Application.DTOs;

/// <summary>Data for creating or updating a topic.</summary>
public class TopicCreateDto
{
    /// <summary>Topic title.</summary>
    [Required]
    [MaxLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description of the topic.</summary>
    [Required]
    [MaxLength(FieldLengths.Description)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Identifier of the (existing) conference the topic belongs to.</summary>
    [Range(1, int.MaxValue)]
    public int ConferenceId { get; set; }
}
