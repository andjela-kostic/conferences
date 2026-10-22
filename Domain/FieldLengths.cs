namespace Domain;

/// <summary>Maximum lengths of text fields, shared by request validation and the database schema.</summary>
public static class FieldLengths
{
    public const int Name = 200;
    public const int Description = 2000;
    public const int PersonName = 100;

    /// <summary>Maximum length of an e-mail address (RFC 5321 path limit).</summary>
    public const int Email = 254;
}

