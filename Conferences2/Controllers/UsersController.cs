using Conferences2.Data;
using Conferences2.Models;
using Conferences2.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conferences2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController: ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
         return await _context.Users.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<User>> RegisterUser(UserCreateDto dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);    
    }

    [HttpPost("register-topic")]
    public async Task<IActionResult> RegisterToTopic(UserTopicDto dto)
    {
        var userExist = await _context.Users.AnyAsync(u=>u.Id == dto.UserId);
        var topicExist = await _context.Topics.AnyAsync(t=>t.Id==dto.TopicId);
        if(!userExist || !topicExist) return BadRequest("User or Topic doesn't exist.");
        
        var alreadyRegistered = await _context.UserTopics.AnyAsync(ut=>ut.UserId ==dto.UserId && ut.TopicId == dto.TopicId);
        if(alreadyRegistered) return BadRequest("The user is already registered on the topic.");

        var userTopic = new UserTopic
        {
            TopicId = dto.TopicId,
            UserId = dto.UserId,
        };
        
        _context.UserTopics.Add(userTopic);
        await _context.SaveChangesAsync();

        return Ok("The user is successfully registered on the topic.");
    }

    [HttpPost("unregister-topic")]
    public async Task<IActionResult> UnregisterFromTopic(UserTopicDto dto) 
    {
        var userTopic = await _context.UserTopics.FirstOrDefaultAsync(ut=>ut.TopicId == dto.TopicId && ut.UserId == dto.UserId);
        if(userTopic== null) return NotFound("The registration does not exist.");
        
        _context.UserTopics.Remove(userTopic);
        await _context.SaveChangesAsync();
        
        return Ok("The user is successfully unregistered from the topic.");
    }

    [HttpGet("{userId}/topics")]
    public async Task<ActionResult<IEnumerable<Topic>>> GetUserTopics(int userId)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if(!userExists) return NotFound($"User with ID {userId} not found.");
        
        var topics = await _context.UserTopics
            .Where(ut => ut.UserId == userId)
            .Select(t => t.Topic)
            .ToListAsync();
        
        if(!topics.Any()) return NotFound($"No topics found for user {userId}.");
        
        return topics;
    }

    [HttpGet("topic/{topicId}")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersByTopics(int topicId)
    {
        var topicExists = await _context.Topics.AnyAsync(t => t.Id == topicId);
        if(!topicExists) return NotFound($"Topic with ID {topicId} not found.");
        
        var users = await _context.UserTopics
            .Where(ut => ut.TopicId == topicId)
            .Select(t => t.User)
            .ToListAsync();
        
        if(!users.Any()) return NotFound($"No users registered for topic {topicId}.");
        
        return users;
    }
}