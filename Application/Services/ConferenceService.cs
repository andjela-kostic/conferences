using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
using Domain.Entities;

namespace Application.Services;

public class ConferenceService : IConferenceService
{
    private readonly IConferenceRepository _conferenceRepository;

    public ConferenceService(IConferenceRepository conferenceRepository)
    {
        _conferenceRepository = conferenceRepository;
    }

    public async Task<IReadOnlyList<ConferenceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var conferences = await _conferenceRepository.GetAllWithTopicsAsync(cancellationToken);
        return [.. conferences.Select(c => c.ToResponse())];
    }

    public async Task<ConferenceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var conference = await _conferenceRepository.GetByIdWithTopicsAsync(id, cancellationToken)
                         ?? throw new NotFoundException($"Conference with ID {id} not found.");

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
        return conference.ToResponse();
    }

    public async Task UpdateAsync(int id, ConferenceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var conference = await _conferenceRepository.GetByIdAsync(id, cancellationToken)
                         ?? throw new NotFoundException($"Conference with ID {id} not found.");

        conference.Name = dto.Name;
        conference.Description = dto.Description;
        conference.Date = dto.Date;

        await _conferenceRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var conference = await _conferenceRepository.GetByIdAsync(id, cancellationToken)
                         ?? throw new NotFoundException($"Conference with ID {id} not found.");

        _conferenceRepository.Remove(conference);
        await _conferenceRepository.SaveChangesAsync(cancellationToken);
    }
}
