using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Training.SkillDevelopment.Configuration;

namespace Training.SkillDevelopment.Controllers.Auth;

/// <summary>
/// Development-only JWT token issuer for shared-frontend integration.
/// Contract matches the team-wide dev-token protocol (see rh-optimerp-frontend Issue #2).
/// In non-Development environments this endpoint returns 403.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class DevAuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevAuthController> _logger;

    public DevAuthController(
        IOptions<JwtSettings> jwtSettings,
        IWebHostEnvironment environment,
        ILogger<DevAuthController> logger)
    {
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Issues a development JWT bearer token for the shared frontend.
    /// Returns 403 outside of the Development environment.
    /// </summary>
    [HttpPost("dev-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DevTokenResponse), 200)]
    [ProducesResponseType(403)]
    public IActionResult IssueDevToken([FromBody] DevTokenRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogWarning("DevAuth: dev-token endpoint called outside Development environment.");
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Dev token endpoint disabled in non-Development environments." });
        }

        if (request is null)
            return BadRequest(new { error = "Request body is required." });

        var now = DateTime.UtcNow;
        var expires = now.AddHours(_jwtSettings.ExpirationHours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, request.UserId ?? "dev-user"),
            new(ClaimTypes.Name, $"{request.FirstName} {request.LastName}".Trim()),
            new(ClaimTypes.Email, request.Email ?? "dev@rh-optimerp.fr"),
            new(ClaimTypes.Role, request.Role ?? "HR"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "DevAuth: issued dev token for user {UserId} ({Role}), expires at {Expires}.",
            request.UserId, request.Role, expires);

        return Ok(new DevTokenResponse
        {
            Token = tokenString,
            ExpiresAt = expires,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        });
    }
}

public sealed record DevTokenRequest
{
    public string? UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Role { get; init; }
}

public sealed record DevTokenResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}
