using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class ConferenceService : IConferenceService
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly ITopicRepository _topicRepository;

    public ConferenceService(IConferenceRepository conferenceRepository, ITopicRepository topicRepository)
    {
        _conferenceRepository = conferenceRepository;
        _topicRepository = topicRepository;
    }

    public async Task<IEnumerable<Conference>> GetAllAsync()
    {
        return await _conferenceRepository.GetAllWithTopicsAsync();
    }

    public async Task<Conference> CreateAsync(ConferenceCreateDto dto)
    {
        var selectedTopics = await _topicRepository.GetByIdsAsync(dto.TopicIds);

        var conference = new Conference
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            Topics = selectedTopics
        };

        _conferenceRepository.Add(conference);
        await _conferenceRepository.SaveChangesAsync();
        return conference;
    }

    public async Task<Conference> UpdateAsync(int id, ConferenceCreateDto dto)
    {
        var conference = await _conferenceRepository.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException($"Conference with ID {id} not found.");

        var selectedTopics = await _topicRepository.GetByIdsAsync(dto.TopicIds);

        conference.Name = dto.Name;
        conference.Description = dto.Description;
        conference.Date = dto.Date;
        conference.Topics = selectedTopics;

        await _conferenceRepository.SaveChangesAsync();
        return conference;
    }

    public async Task DeleteAsync(int id)
    {
        var conference = await _conferenceRepository.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException($"Conference with ID {id} not found.");

        _conferenceRepository.Remove(conference);
        await _conferenceRepository.SaveChangesAsync();
    }
}