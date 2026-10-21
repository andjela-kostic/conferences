using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConferencesController : ControllerBase
{
    private readonly IConferenceService _conferenceService;

    public ConferencesController(IConferenceService conferenceService)
    {
        _conferenceService = conferenceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Conference>>> GetConferences()
    {
        var conferences = await _conferenceService.GetAllAsync();
        return Ok(conferences);
    }

    [HttpPost]
    public async Task<ActionResult<Conference>> CreateConference(ConferenceCreateDto dto)
    {
        var conference = await _conferenceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetConferences), new { id = conference.Id }, conference);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConference(int id, ConferenceCreateDto dto)
    {
        await _conferenceService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConference(int id)
    {
        await _conferenceService.DeleteAsync(id);
        return NoContent();
    }
}

