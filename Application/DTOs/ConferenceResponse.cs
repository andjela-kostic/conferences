namespace Application.DTOs;

/// <summary>A conference with the topics that belong to it.</summary>
/// <param name="Id">Conference identifier.</param>
/// <param name="Name">Conference name.</param>
/// <param name="Description">Short description of the conference.</param>
/// <param name="Date">Start date and time (UTC).</param>
/// <param name="Topics">Topics presented at the conference.</param>
public sealed record ConferenceResponse(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    IReadOnlyList<TopicResponse> Topics);


