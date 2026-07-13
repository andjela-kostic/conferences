using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface IConferenceService
{
    Task<IEnumerable<Conference>> GetAllAsync();
    Task<Conference> CreateAsync(ConferenceCreateDto dto);
    Task<Conference> UpdateAsync(int id, ConferenceCreateDto dto);
    Task DeleteAsync(int id);
}