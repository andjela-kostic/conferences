using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Logging;
using Application.Mappings;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ConferenceService : IConferenceService
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly ILogger<ConferenceService> _logger;

    public ConferenceService(IConferenceRepository conferenceRepository, ILogger<ConferenceService> logger)
    {
        _conferenceRepository = conferenceRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ConferenceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var conferences = await _conferenceRepository.GetAllWithTopicsAsync(cancellationToken);
        return [.. conferences.Select(c => c.ToResponse())];
    }

    public async Task<ConferenceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var conference = await _conferenceRepository.GetByIdWithTopicsAsync(id, cancellationToken)
                         ?? throw NotFound(id);

        return conference.ToResponse();
    }

    public async Task<ConferenceResponse> CreateAsync(ConferenceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var conference = new Conference
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
        };

        _conferenceRepository.Add(conference);
        await _conferenceRepository.SaveChangesAsync(cancellationToken);

        _logger.ConferenceCreated(conference.Id);
        return conference.ToResponse();
    }

    public async Task UpdateAsync(int id, ConferenceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var conference = await _conferenceRepository.GetByIdAsync(id, cancellationToken)
                         ?? throw NotFound(id);

        conference.Name = dto.Name;
        conference.Description = dto.Description;
        conference.Date = dto.Date;

        await _conferenceRepository.SaveChangesAsync(cancellationToken);
        _logger.ConferenceUpdated(id);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var conference = await _conferenceRepository.GetByIdAsync(id, cancellationToken)
                         ?? throw NotFound(id);

        _conferenceRepository.Remove(conference);
        await _conferenceRepository.SaveChangesAsync(cancellationToken);
        _logger.ConferenceDeleted(id);
    }

    private NotFoundException NotFound(int id)
    {
        _logger.ConferenceNotFound(id);
        return new NotFoundException($"Conference with ID {id} not found.");
    }
}
