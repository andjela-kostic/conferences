namespace Application.DTOs;

/// <summary>Data for creating or updating a conference.</summary>
public class ConferenceCreateDto
{
    /// <summary>Conference name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description of the conference.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Start date and time (UTC, e.g. <c>2027-06-01T09:00:00Z</c>).</summary>
    public DateTime Date { get; set; }

    /// <summary>Identifiers of existing topics to attach to the conference.</summary>
    public List<int> TopicIds { get; set; } = new();
}
