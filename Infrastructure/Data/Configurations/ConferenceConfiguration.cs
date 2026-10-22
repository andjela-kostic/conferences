using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class ConferenceConfiguration : IEntityTypeConfiguration<Conference>
{
    public void Configure(EntityTypeBuilder<Conference> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(FieldLengths.Name);
        builder.Property(c => c.Description).HasMaxLength(FieldLengths.Description);
    }
}

