using System.ComponentModel.DataAnnotations;

namespace Application.Validation;

/// <summary>
/// Accepts only UTC date-times (ISO 8601 with a trailing <c>Z</c>). Values without a time zone are ambiguous,
/// and values with an offset would be shifted by the server's local time zone.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class UtcDateTimeAttribute : ValidationAttribute
{
    public UtcDateTimeAttribute()
        : base("The {0} field must be a UTC date and time ending in 'Z', e.g. 2027-06-01T09:00:00Z.")
    {
    }

    public override bool IsValid(object? value) => value is null or DateTime { Kind: DateTimeKind.Utc };
}

