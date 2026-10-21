using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Topic>>> GetTopics()
    {
        var topics = await _topicService.GetAllAsync();
        return Ok(topics);
    }

    [HttpGet("conference/{conferenceId}")]
    public async Task<ActionResult<IEnumerable<Topic>>> GetTopicsByConference(int conferenceId)
    {
        var topics = await _topicService.GetByConferenceAsync(conferenceId);
        return Ok(topics);
    }

    [HttpPost]
    public async Task<ActionResult<Topic>> CreateTopic(TopicCreateDto dto)
    {
        var topic = await _topicService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetTopics), new { id = topic.Id }, topic);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTopic(int id, TopicCreateDto dto)
    {
        await _topicService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        await _topicService.DeleteAsync(id);
        return NoContent();
    }
}

