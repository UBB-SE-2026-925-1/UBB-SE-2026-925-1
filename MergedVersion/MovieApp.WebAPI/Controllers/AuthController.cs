using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MovieApp.Core.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MovieApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly IUserRepository userRepository;

    public AuthController(IConfiguration configuration, IUserRepository userRepository)
    {
        this.configuration = configuration;
        this.userRepository = userRepository;
    }

    public record LoginRequest(string Username, string Password);

    public record LoginResponse(string Token);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Validate against hardcoded bootstrap credentials from config

        var expectedUsername = this.configuration["BootstrapUser:Username"];
        var expectedPassword = this.configuration["BootstrapUser:Password"];

        if (request.Username != expectedUsername || request.Password != expectedPassword)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Load the actual User entity so we can embed the real DB id in the token
        var user = await this.userRepository.FindByAuthIdentityAsync("Seed", "Admin");
        if (user is null)
        {
            return StatusCode(500, "Bootstrap user not found in database.");
        }

        var token = GenerateToken(user.Id, user.Username);
        return Ok(new LoginResponse(token));
    }

    private string GenerateToken(int userId, string username)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(this.configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: this.configuration["Jwt:Issuer"],
            audience: this.configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(this.configuration["Jwt:ExpiryMinutes"]!)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);


    }
}