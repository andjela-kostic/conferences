using Microsoft.Extensions.Logging;

namespace Application.Logging;

/// <summary>
/// All log messages of the application layer, generated at compile time by <c>[LoggerMessage]</c>
/// (no boxing or message formatting when the level is disabled).
/// Parameters are structured properties (e.g. <c>ConferenceId</c>) for filtering in a log store.
/// Personal data such as e-mail addresses or names is never logged; users are identified by <c>UserId</c> only.
/// </summary>
/// <remarks>
/// Event ID ranges: 1xxx conferences, 2xxx topics, 3xxx users, 4xxx topic registrations.
/// A missing resource is an expected client error (404), so it is logged as Information; conflicts (409) as Warning.
/// Unexpected exceptions are logged as Error by the ASP.NET Core exception handler middleware.
/// </remarks>
internal static partial class Log
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Created conference {ConferenceId}")]
    public static partial void ConferenceCreated(this ILogger logger, int conferenceId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Updated conference {ConferenceId}")]
    public static partial void ConferenceUpdated(this ILogger logger, int conferenceId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Deleted conference {ConferenceId} together with its topics")]
    public static partial void ConferenceDeleted(this ILogger logger, int conferenceId);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Conference {ConferenceId} not found")]
    public static partial void ConferenceNotFound(this ILogger logger, int conferenceId);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Created topic {TopicId} in conference {ConferenceId}")]
    public static partial void TopicCreated(this ILogger logger, int topicId, int conferenceId);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Updated topic {TopicId} in conference {ConferenceId}")]
    public static partial void TopicUpdated(this ILogger logger, int topicId, int conferenceId);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Deleted topic {TopicId} together with its registrations")]
    public static partial void TopicDeleted(this ILogger logger, int topicId);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Topic {TopicId} not found")]
    public static partial void TopicNotFound(this ILogger logger, int topicId);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Registered user {UserId}")]
    public static partial void UserRegistered(this ILogger logger, int userId);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = "Updated user {UserId}")]
    public static partial void UserUpdated(this ILogger logger, int userId);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, Message = "Deleted user {UserId} together with their registrations")]
    public static partial void UserDeleted(this ILogger logger, int userId);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Information, Message = "User {UserId} not found")]
    public static partial void UserNotFound(this ILogger logger, int userId);

    [LoggerMessage(EventId = 3005, Level = LogLevel.Warning, Message = "Rejected e-mail address for user {UserId}: it is already used by another user")]
    public static partial void UserEmailTaken(this ILogger logger, int? userId);

    [LoggerMessage(EventId = 4001, Level = LogLevel.Information, Message = "User {UserId} registered for topic {TopicId}")]
    public static partial void UserRegisteredForTopic(this ILogger logger, int userId, int topicId);

    [LoggerMessage(EventId = 4002, Level = LogLevel.Information, Message = "User {UserId} unregistered from topic {TopicId}")]
    public static partial void UserUnregisteredFromTopic(this ILogger logger, int userId, int topicId);

    [LoggerMessage(EventId = 4003, Level = LogLevel.Warning, Message = "User {UserId} is already registered for topic {TopicId}")]
    public static partial void UserAlreadyRegisteredForTopic(this ILogger logger, int userId, int topicId);

    [LoggerMessage(EventId = 4004, Level = LogLevel.Information, Message = "Registration of user {UserId} for topic {TopicId} not found")]
    public static partial void RegistrationNotFound(this ILogger logger, int userId, int topicId);

    [LoggerMessage(EventId = 4005, Level = LogLevel.Information,
        Message = "Cannot register user {UserId} for topic {TopicId}: user exists = {UserExists}, topic exists = {TopicExists}")]
    public static partial void RegistrationTargetNotFound(this ILogger logger, int userId, int topicId, bool userExists, bool topicExists);
}

