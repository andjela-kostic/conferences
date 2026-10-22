using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
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

    public async Task<IReadOnlyList<TopicResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var topics = await _topicRepository.GetAllAsync(cancellationToken);
        return [.. topics.Select(t => t.ToResponse())];
    }

    public async Task<TopicResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Topic with ID {id} not found.");

        return topic.ToResponse();
    }

    public async Task<IReadOnlyList<TopicResponse>> GetByConferenceAsync(int conferenceId, CancellationToken cancellationToken = default)
    {
        await EnsureConferenceExistsAsync(conferenceId, cancellationToken);

        var topics = await _topicRepository.GetByConferenceIdAsync(conferenceId, cancellationToken);
        return [.. topics.Select(t => t.ToResponse())];
    }

    public async Task<TopicResponse> CreateAsync(TopicCreateDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureConferenceExistsAsync(dto.ConferenceId, cancellationToken);

        var topic = new Topic
        {
            Name = dto.Name,
            Description = dto.Description,
            ConferenceId = dto.ConferenceId
        };

        _topicRepository.Add(topic);
        await _topicRepository.SaveChangesAsync(cancellationToken);
        return topic.ToResponse();
    }

    public async Task UpdateAsync(int id, TopicCreateDto dto, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Topic with ID {id} not found.");

        await EnsureConferenceExistsAsync(dto.ConferenceId, cancellationToken);

        topic.Name = dto.Name;
        topic.Description = dto.Description;
        topic.ConferenceId = dto.ConferenceId;

        await _topicRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Topic with ID {id} not found.");

        _topicRepository.Remove(topic);
        await _topicRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureConferenceExistsAsync(int conferenceId, CancellationToken cancellationToken)
    {
        if (!await _conferenceRepository.ExistsAsync(conferenceId, cancellationToken))
            throw new NotFoundException($"Conference with ID {conferenceId} not found.");
    }
}
