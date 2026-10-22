namespace Application.DTOs;

/// <summary>A registered user (conference attendee).</summary>
/// <param name="Id">User identifier.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="Email">E-mail address.</param>
public sealed record UserResponse(int Id, string FirstName, string LastName, string Email);


