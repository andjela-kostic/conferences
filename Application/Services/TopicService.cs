using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly IConferenceRepository _conferenceRepository;

    public TopicService(ITopicRepository topicRepository, IConferenceRepository conferenceRepository)
    {
        _topicRepository = topicRepository;
        _conferenceRepository = conferenceRepository;
    }

    public async Task<IEnumerable<Topic>> GetAllAsync()
    {
        return await _topicRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Topic>> GetByConferenceAsync(int conferenceId)
    {
        var conference = await _conferenceRepository.GetByIdAsync(conferenceId);
        if (conference is null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found.");

        return await _topicRepository.GetByConferenceIdAsync(conferenceId);
    }

    public async Task<Topic> CreateAsync(TopicCreateDto dto)
    {
        var conference = await _conferenceRepository.GetByIdAsync(dto.ConferenceId);
        if (conference is null)
            throw new KeyNotFoundException($"Conference with ID {dto.ConferenceId} not found.");

        var topic = new Topic
        {
            Name = dto.Name,
            Description = dto.Description,
            ConferenceId = dto.ConferenceId
        };

        _topicRepository.Add(topic);
        await _topicRepository.SaveChangesAsync();
        return topic;
    }

    public async Task<Topic> UpdateAsync(int id, TopicCreateDto dto)
    {
        var topic = await _topicRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Topic with ID {id} not found.");

        var conference = await _conferenceRepository.GetByIdAsync(dto.ConferenceId);
        if (conference is null)
            throw new KeyNotFoundException($"Conference with ID {dto.ConferenceId} not found.");

        topic.Name = dto.Name;
        topic.Description = dto.Description;
        topic.ConferenceId = dto.ConferenceId;

        await _topicRepository.SaveChangesAsync();
        return topic;
    }

    public async Task DeleteAsync(int id)
    {
        var topic = await _topicRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Topic with ID {id} not found.");

        _topicRepository.Remove(topic);
        await _topicRepository.SaveChangesAsync();
    }
}