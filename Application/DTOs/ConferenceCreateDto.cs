using System.ComponentModel.DataAnnotations;
using Application.Validation;
using Domain;

namespace Application.DTOs;

/// <summary>Data for creating or updating a conference. Topics are managed through <c>/api/topics</c>.</summary>
public class ConferenceCreateDto
{
    /// <summary>Conference name.</summary>
    [Required]
    [MaxLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description of the conference.</summary>
    [Required]
    [MaxLength(FieldLengths.Description)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Start date and time in UTC, e.g. <c>2027-06-01T09:00:00Z</c>.</summary>
    [UtcDateTime]
    public DateTime Date { get; set; }
}
