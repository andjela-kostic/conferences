using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings;

public static class ResponseMappings
{
    public static ConferenceResponse ToResponse(this Conference conference) => new(
        conference.Id,
        conference.Name,
        conference.Description,
        conference.Date,
        [.. conference.Topics.Select(t => t.ToResponse())]);

    public static TopicResponse ToResponse(this Topic topic) =>
        new(topic.Id, topic.Name, topic.Description, topic.ConferenceId);

    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email);
}

