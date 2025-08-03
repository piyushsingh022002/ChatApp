using ChatApp.Api.Models;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly MongoService _mongo;
    private readonly JwtService _jwt;
    private readonly PasswordHasher _hasher;
    private readonly IMongoCollection<User> _users;

    public AuthController(MongoService mongo, JwtService jwt, PasswordHasher hasher)
    {
        _mongo = mongo;
        _jwt = jwt;
        _hasher = hasher;
        _users = _mongo.GetCollection<User>("Users");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        var existing = await _users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
        if (existing != null) return BadRequest("Email already in use.");

        var user = new User
        {
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = _hasher.Hash(dto.Password)
        };

        await _users.InsertOneAsync(user);
        var token = _jwt.GenerateToken(user.Username);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var user = await _users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = _jwt.GenerateToken(user.Username);
        return Ok(new { token });
    }
}
public class UserRegisterDto
{
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class UserLoginDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
