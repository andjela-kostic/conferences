using Conferences.Data;
using Conferences.Models;
using Conferences.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conferences.Controllers;

[ApiController] 
[Route("api/[controller]")] 
public class ConferencesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConferencesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Conference>>> GetConferences()
    {
        return await _context.Conferences.Include(c=>c.Topics).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Conference>> CreateConference(ConferenceCreateDto dto)
    {
        var selectedTopics = await _context.Topics
            .Where(t => dto.TopicIds.Contains(t.Id))
            .ToListAsync();
        
        var conference = new Conference
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            Topics = selectedTopics
        };
        
        _context.Conferences.Add(conference);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConferences), new { id = conference.Id }, conference);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConference(int id, ConferenceCreateDto dto)
    {
        var conference = await _context.Conferences.FindAsync(id);
        if(conference == null) return NotFound();
        
        conference.Name = dto.Name;
        conference.Description = dto.Description;
        conference.Date = dto.Date;

        var selectedTopics = await _context.Topics
            .Where(t => dto.TopicIds.Contains(t.Id))
            .ToListAsync();
        
        conference.Topics = selectedTopics;
        
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConference(int id)
    {
        var conference = await _context.Conferences.FindAsync(id);
        if(conference == null) return NotFound();
        
        _context.Conferences.Remove(conference);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}