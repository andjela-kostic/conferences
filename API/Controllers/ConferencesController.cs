using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>Conferences and the topics presented at them.</summary>
[ApiController]
[Route("api/conferences")]
public class ConferencesController : ControllerBase
{
    private readonly IConferenceService _conferenceService;

    public ConferencesController(IConferenceService conferenceService)
    {
        _conferenceService = conferenceService;
    }

    /// <summary>Lists all conferences with their topics.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ConferenceResponse>>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult<IReadOnlyList<ConferenceResponse>>> GetConferences(CancellationToken cancellationToken)
    {
        return Ok(await _conferenceService.GetAllAsync(cancellationToken));
    }

    /// <summary>Gets a single conference with its topics.</summary>
    /// <param name="id">Conference identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="404">The conference does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ConferenceResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<ConferenceResponse>> GetConference(int id, CancellationToken cancellationToken)
    {
        return Ok(await _conferenceService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Creates a conference, optionally attaching existing topics.</summary>
    /// <response code="201">Created; the <c>Location</c> header points to the new conference.</response>
    /// <response code="400">The request body is invalid.</response>
    [HttpPost]
    [ProducesResponseType<ConferenceResponse>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<ConferenceResponse>> CreateConference(ConferenceCreateDto dto, CancellationToken cancellationToken)
    {
        var conference = await _conferenceService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetConference), new { id = conference.Id }, conference);
    }

    /// <summary>Updates a conference.</summary>
    /// <param name="id">Conference identifier.</param>
    /// <param name="dto">New values.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="204">Updated.</response>
    /// <response code="404">The conference does not exist.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> UpdateConference(int id, ConferenceCreateDto dto, CancellationToken cancellationToken)
    {
        await _conferenceService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a conference together with its topics.</summary>
    /// <param name="id">Conference identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="204">Deleted.</response>
    /// <response code="404">The conference does not exist.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> DeleteConference(int id, CancellationToken cancellationToken)
    {
        await _conferenceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

