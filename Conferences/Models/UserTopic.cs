namespace Conferences.Models;

public class UserTopic
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TopicId { get; set; }

    public User User { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
}