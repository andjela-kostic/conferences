using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Logging;
using Application.Mappings;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly IConferenceRepository _conferenceRepository;
    private readonly ILogger<TopicService> _logger;

    public TopicService(ITopicRepository topicRepository, IConferenceRepository conferenceRepository, ILogger<TopicService> logger)
    {
        _topicRepository = topicRepository;
        _conferenceRepository = conferenceRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TopicResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var topics = await _topicRepository.GetAllAsync(cancellationToken);
        return [.. topics.Select(t => t.ToResponse())];
    }

    public async Task<TopicResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken)
            ?? throw TopicNotFound(id);

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

        _logger.TopicCreated(topic.Id, topic.ConferenceId);
        return topic.ToResponse();
    }

    public async Task UpdateAsync(int id, TopicCreateDto dto, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken)
            ?? throw TopicNotFound(id);

        await EnsureConferenceExistsAsync(dto.ConferenceId, cancellationToken);

        topic.Name = dto.Name;
        topic.Description = dto.Description;
        topic.ConferenceId = dto.ConferenceId;

        await _topicRepository.SaveChangesAsync(cancellationToken);
        _logger.TopicUpdated(id, topic.ConferenceId);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken)
            ?? throw TopicNotFound(id);

        _topicRepository.Remove(topic);
        await _topicRepository.SaveChangesAsync(cancellationToken);
        _logger.TopicDeleted(id);
    }

    private async Task EnsureConferenceExistsAsync(int conferenceId, CancellationToken cancellationToken)
    {
        if (!await _conferenceRepository.ExistsAsync(conferenceId, cancellationToken))
        {
            _logger.ConferenceNotFound(conferenceId);
            throw new NotFoundException($"Conference with ID {conferenceId} not found.");
        }
    }

    private NotFoundException TopicNotFound(int id)
    {
        _logger.TopicNotFound(id);
        return new NotFoundException($"Topic with ID {id} not found.");
    }
}

