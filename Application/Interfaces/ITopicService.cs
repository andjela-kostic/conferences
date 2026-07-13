using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface ITopicService
{
    Task<IEnumerable<Topic>> GetAllAsync();
    Task<IEnumerable<Topic>> GetByConferenceAsync(int conferenceId);
    Task<Topic> CreateAsync(TopicCreateDto dto);
    Task<Topic> UpdateAsync(int id, TopicCreateDto dto);
    Task DeleteAsync(int id);
}