using System.ComponentModel.DataAnnotations;
using Domain;

namespace Application.DTOs;

/// <summary>Data for registering or updating a user.</summary>
public class UserCreateDto
{
    /// <summary>First name.</summary>
    [Required]
    [MaxLength(FieldLengths.PersonName)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name.</summary>
    [Required]
    [MaxLength(FieldLengths.PersonName)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>E-mail address; unique and case-insensitive (stored trimmed and in lowercase).</summary>
    [Required]
    [EmailAddress]
    [MaxLength(FieldLengths.Email)]
    public string Email { get; set; } = string.Empty;
}
