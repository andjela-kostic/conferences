using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class UserTopicConfiguration : IEntityTypeConfiguration<UserTopic>
{
    public const string UserTopicIndex = "IX_UserTopics_UserId_TopicId";

    public void Configure(EntityTypeBuilder<UserTopic> builder)
    {
        builder.HasIndex(ut => new { ut.UserId, ut.TopicId }).IsUnique().HasDatabaseName(UserTopicIndex);
    }
}

