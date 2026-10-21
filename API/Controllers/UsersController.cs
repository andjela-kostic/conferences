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
        await _userService.RegisterToTopicAsync(dto);
        return Ok("The user is successfully registered on the topic.");
    }

    [HttpPost("unregister-topic")]
    public async Task<IActionResult> UnregisterFromTopic(UserTopicDto dto)
    {
        await _userService.UnregisterFromTopicAsync(dto);
        return Ok("The user is successfully unregistered from the topic.");
    }

    [HttpGet("{userId}/topics")]
    public async Task<ActionResult<IEnumerable<Topic>>> GetUserTopics(int userId)
    {
        var topics = await _userService.GetUserTopicsAsync(userId);
        return Ok(topics);
    }

    [HttpGet("topic/{topicId}")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersByTopics(int topicId)
    {
        var users = await _userService.GetUsersByTopicAsync(topicId);
        return Ok(users);
    }
}

