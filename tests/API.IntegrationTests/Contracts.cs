namespace API.IntegrationTests;

public sealed record ConferenceResponse(int Id, string Name, string Description, DateTime Date, List<TopicResponse> Topics);

public sealed record TopicResponse(int Id, string Name, string Description, int ConferenceId);

public sealed record UserResponse(int Id, string FirstName, string LastName, string Email);

