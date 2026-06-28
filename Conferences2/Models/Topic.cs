namespace Conferences2.Models;

public class Topic
{
    public int Id { get; set; }
    public string Name { get; set; }= string.Empty;
    public string Description { get; set; }= string.Empty;
    public int ConferenceId { get; set; }

    public Conference? Conference { get; set; } = null!;
    public ICollection<UserTopic> UserTopics { get; set; } = new List<UserTopic>();
}