using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<User>> RegisterUser(UserCreateDto dto)
    {
        var user = await _userService.RegisterUserAsync(dto);
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }

    [HttpPost("register-topic")]
    public async Task<IActionResult> RegisterToTopic(UserTopicDto dto)
    {
        try
        {
            await _userService.RegisterToTopicAsync(dto);
            return Ok("The user is successfully registered on the topic.");
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("unregister-topic")]
    public async Task<IActionResult> UnregisterFromTopic(UserTopicDto dto)
    {
        try
        {
            await _userService.UnregisterFromTopicAsync(dto);
            return Ok("The user is successfully unregistered from the topic.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{userId}/topics")]
    public async Task<ActionResult<IEnumerable<Topic>>> GetUserTopics(int userId)
    {
        try
        {
            var topics = await _userService.GetUserTopicsAsync(userId);
            return Ok(topics);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("topic/{topicId}")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersByTopics(int topicId)
    {
        try
        {
            var users = await _userService.GetUsersByTopicAsync(topicId);
            return Ok(users);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

