namespace Application.DTOs;

/// <summary>A topic presented at a conference.</summary>
/// <param name="Id">Topic identifier.</param>
/// <param name="Name">Topic title.</param>
/// <param name="Description">Short description of the topic.</param>
/// <param name="ConferenceId">Identifier of the conference the topic belongs to.</param>
public sealed record TopicResponse(int Id, string Name, string Description, int ConferenceId);


