namespace Application.DTOs;

/// <summary>Data for registering or updating a user.</summary>
public class UserCreateDto
{
    /// <summary>First name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>E-mail address.</summary>
    public string Email { get; set; } = string.Empty;
}
