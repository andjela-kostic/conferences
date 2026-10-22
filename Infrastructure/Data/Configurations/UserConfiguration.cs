using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const string EmailIndex = "IX_Users_Email";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(FieldLengths.PersonName);
        builder.Property(u => u.LastName).HasMaxLength(FieldLengths.PersonName);
        builder.Property(u => u.Email).HasMaxLength(FieldLengths.Email);

        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName(EmailIndex);
    }
}

