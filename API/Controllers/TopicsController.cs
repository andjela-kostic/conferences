using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>Topics presented at conferences.</summary>
[ApiController]
[Route("api/topics")]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    /// <summary>Lists all topics.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TopicResponse>>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult<IReadOnlyList<TopicResponse>>> GetTopics(CancellationToken cancellationToken)
    {
        return Ok(await _topicService.GetAllAsync(cancellationToken));
    }

    /// <summary>Gets a single topic.</summary>
    /// <param name="id">Topic identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="404">The topic does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TopicResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TopicResponse>> GetTopic(int id, CancellationToken cancellationToken)
    {
        return Ok(await _topicService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Lists the topics of a conference.</summary>
    /// <param name="conferenceId">Conference identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="404">The conference does not exist.</response>
    [HttpGet("conference/{conferenceId:int}")]
    [ProducesResponseType<IReadOnlyList<TopicResponse>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<IReadOnlyList<TopicResponse>>> GetTopicsByConference(int conferenceId, CancellationToken cancellationToken)
    {
        return Ok(await _topicService.GetByConferenceAsync(conferenceId, cancellationToken));
    }

    /// <summary>Creates a topic in an existing conference.</summary>
    /// <response code="201">Created; the <c>Location</c> header points to the new topic.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="404">The conference does not exist.</response>
    [HttpPost]
    [ProducesResponseType<TopicResponse>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TopicResponse>> CreateTopic(TopicCreateDto dto, CancellationToken cancellationToken)
    {
        var topic = await _topicService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetTopic), new { id = topic.Id }, topic);
    }

    /// <summary>Updates a topic; changing <c>conferenceId</c> moves it to another conference.</summary>
    /// <param name="id">Topic identifier.</param>
    /// <param name="dto">New values.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="204">Updated.</response>
    /// <response code="404">The topic or the conference does not exist.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> UpdateTopic(int id, TopicCreateDto dto, CancellationToken cancellationToken)
    {
        await _topicService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a topic together with its registrations.</summary>
    /// <param name="id">Topic identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="204">Deleted.</response>
    /// <response code="404">The topic does not exist.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> DeleteTopic(int id, CancellationToken cancellationToken)
    {
        await _topicService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

