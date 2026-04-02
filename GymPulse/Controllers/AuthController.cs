using GymPulse.DTOs.Requests;
using GymPulse.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymPulse.Controllers;

/// <summary>
/// Handles login for Members and Trainers, returning a JWT token on success.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ITrainerService _trainerService;

    public AuthController(IMemberService memberService, ITrainerService trainerService)
    {
        _memberService = memberService;
        _trainerService = trainerService;
    }

    /// <summary>Register a new member account.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterMemberDto dto)
    {
        // ModelState is automatically validated via [ApiController] + Data Annotations
        // If invalid → ASP.NET Core returns HTTP 400 before we even get here
        var result = await _memberService.RegisterAsync(dto);
        return CreatedAtAction(nameof(Register), new { id = result.Id }, result);
    }

    /// <summary>Login as a Member. Returns a JWT token.</summary>
    [HttpPost("login/member")]
    public async Task<IActionResult> LoginMember([FromBody] LoginDto dto)
    {
        var result = await _memberService.LoginAsync(dto);
        if (result is null) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(result);
    }

    /// <summary>Login as a Trainer. Returns a JWT token.</summary>
    [HttpPost("login/trainer")]
    public async Task<IActionResult> LoginTrainer([FromBody] LoginDto dto)
    {
        var result = await _trainerService.LoginAsync(dto);
        if (result is null) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(result);
    }
}
