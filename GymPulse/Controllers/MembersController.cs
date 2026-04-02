using GymPulse.DTOs.Requests;
using GymPulse.Helpers;
using GymPulse.Services;
using GymPulse.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymPulse.Controllers;

/// <summary>
/// Manages member accounts, profiles, and subscriptions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]  // All endpoints require a valid JWT token by default
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ISubscriptionService _subscriptionService;

    public MembersController(IMemberService memberService, ISubscriptionService subscriptionService)
    {
        _memberService = memberService;
        _subscriptionService = subscriptionService;
    }

    // ─── Member CRUD ──────────────────────────────────────────────────────────

    /// <summary>Get all members. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _memberService.GetAllAsync());

    /// <summary>Get a member by ID. Admin or the member themselves.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        // A member can only view their own data; Admin can view anyone
        var requesterId = JwtHelper.GetUserId(User);
        if (!User.IsInRole("Admin") && requesterId != id)
            return Forbid();

        var result = await _memberService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Update a member's name or phone. Admin or self.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        var requesterId = JwtHelper.GetUserId(User);
        if (!User.IsInRole("Admin") && requesterId != id)
            return Forbid();

        var result = await _memberService.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a member. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _memberService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Profile (One-to-One) ─────────────────────────────────────────────────

    /// <summary>Get a member's fitness profile.</summary>
    [HttpGet("{id:int}/profile")]
    public async Task<IActionResult> GetProfile(int id)
    {
        var result = await _memberService.GetProfileAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create or update a member's fitness profile.</summary>
    [HttpPut("{id:int}/profile")]
    public async Task<IActionResult> UpsertProfile(int id, [FromBody] UpdateMemberProfileDto dto)
    {
        var requesterId = JwtHelper.GetUserId(User);
        if (!User.IsInRole("Admin") && requesterId != id)
            return Forbid();

        var result = await _memberService.UpsertProfileAsync(id, dto);
        return Ok(result);
    }

    // ─── Subscriptions (One-to-Many) ─────────────────────────────────────────

    /// <summary>Get all subscriptions for a member.</summary>
    [HttpGet("{id:int}/subscriptions")]
    public async Task<IActionResult> GetSubscriptions(int id) =>
        Ok(await _subscriptionService.GetByMemberAsync(id));

    /// <summary>Add a new subscription for a member. Admin only.</summary>
    [HttpPost("{id:int}/subscriptions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddSubscription(int id, [FromBody] CreateSubscriptionDto dto)
    {
        try
        {
            var result = await _subscriptionService.CreateAsync(id, dto);
            return CreatedAtAction(nameof(GetSubscriptions), new { id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Cancel a subscription. Admin only.</summary>
    [HttpDelete("{id:int}/subscriptions/{subscriptionId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelSubscription(int id, int subscriptionId)
    {
        var cancelled = await _subscriptionService.CancelAsync(id, subscriptionId);
        return cancelled ? NoContent() : NotFound();
    }
}
