using Conferences2.Data;
using Conferences2.Models;
using Conferences2.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conferences2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController: ControllerBase
{
    private readonly AppDbContext _context;

    public TopicsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Topic>>> GetTopics()
    {
        return await _context.Topics.ToListAsync();
    }

    [HttpGet("conference/{conferenceId}")]
    public async Task<ActionResult<IEnumerable<Topic>>> GetTopicsByConference(int conferenceId)
    {
        return await _context.Topics.Where(t=>t.ConferenceId == conferenceId).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Topic>> CreateTopic(TopicCreateDto dto)
    {
        var conferenceExists = await _context.Conferences.AnyAsync(c => c.Id == dto.ConferenceId);
        if(!conferenceExists) return BadRequest("Conference exists");

        var topic = new Topic
        {
            Name = dto.Name,
            Description = dto.Description,
            ConferenceId = dto.ConferenceId,
        };
        
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetTopics),new{id=topic.Id}, topic);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Topic>> UpdateTopic(int id, TopicCreateDto dto)
    {
        var topic = await _context.Topics.FindAsync(id);
        if(topic == null) return  NotFound();
        
        topic.Name = dto.Name;
        topic.Description = dto.Description;
        topic.ConferenceId = dto.ConferenceId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if(topic == null) return NotFound();
        
        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}