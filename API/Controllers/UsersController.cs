using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>Users (conference attendees) and their topic registrations.</summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Lists all users.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }

    /// <summary>Gets a single user.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="404">The user does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<UserResponse>> GetUser(int id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Registers a new user.</summary>
    /// <response code="201">Created; the <c>Location</c> header points to the new user.</response>
    /// <response code="400">The request body is invalid.</response>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<UserResponse>> RegisterUser(UserCreateDto dto, CancellationToken cancellationToken)
    {
        var user = await _userService.RegisterUserAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>Updates a user.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="dto">New values.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="204">Updated.</response>
    /// <response code="404">The user does not exist.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> UpdateUser(int id, UserCreateDto dto, CancellationToken cancellationToken)
    {
        await _userService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a user together with their topic registrations.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="204">Deleted.</response>
    /// <response code="404">The user does not exist.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Registers a user for a topic.</summary>
    /// <response code="204">Registered.</response>
    /// <response code="404">The user or the topic does not exist.</response>
    /// <response code="409">The user is already registered for the topic.</response>
    [HttpPost("register-topic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> RegisterToTopic(UserTopicDto dto, CancellationToken cancellationToken)
    {
        await _userService.RegisterToTopicAsync(dto, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes a user's registration for a topic.</summary>
    /// <response code="204">Unregistered.</response>
    /// <response code="404">The registration does not exist.</response>
    [HttpPost("unregister-topic")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> UnregisterFromTopic(UserTopicDto dto, CancellationToken cancellationToken)
    {
        await _userService.UnregisterFromTopicAsync(dto, cancellationToken);
        return NoContent();
    }

    /// <summary>Lists the topics a user is registered for.</summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="404">The user does not exist.</response>
    [HttpGet("{userId:int}/topics")]
    [ProducesResponseType<IReadOnlyList<TopicResponse>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<IReadOnlyList<TopicResponse>>> GetUserTopics(int userId, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetUserTopicsAsync(userId, cancellationToken));
    }

    /// <summary>Lists the users registered for a topic.</summary>
    /// <param name="topicId">Topic identifier.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="404">The topic does not exist.</response>
    [HttpGet("topic/{topicId:int}")]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsersByTopic(int topicId, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetUsersByTopicAsync(topicId, cancellationToken));
    }
}










