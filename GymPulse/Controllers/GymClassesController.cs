using GymPulse.DTOs.Requests;
using GymPulse.Helpers;
using GymPulse.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymPulse.Controllers;

/// <summary>
/// Manages gym classes and member bookings (Many-to-Many).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GymClassesController : ControllerBase
{
    private readonly IGymClassService _classService;

    public GymClassesController(IGymClassService classService) =>
        _classService = classService;

    /// <summary>Get all gym classes. Any authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _classService.GetAllAsync());

    /// <summary>Get a specific gym class by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _classService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new gym class. Admin or Trainer only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Trainer")]
    public async Task<IActionResult> Create([FromBody] CreateGymClassDto dto)
    {
        var result = await _classService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a gym class. Admin or Trainer only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Trainer")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGymClassDto dto)
    {
        var result = await _classService.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a gym class. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _classService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Bookings (Many-to-Many) ──────────────────────────────────────────────

    /// <summary>Get all bookings for a class. Admin or Trainer only.</summary>
    [HttpGet("{id:int}/bookings")]
    [Authorize(Roles = "Admin,Trainer")]
    public async Task<IActionResult> GetBookings(int id) =>
        Ok(await _classService.GetBookingsForClassAsync(id));

    /// <summary>Book a class for the currently logged-in member.</summary>
    [HttpPost("{id:int}/book")]
    [Authorize(Roles = "Member")]
    public async Task<IActionResult> Book(int id)
    {
        // Get the member's ID from their JWT token claims
        var memberId = JwtHelper.GetUserId(User);
        try
        {
            var result = await _classService.BookAsync(memberId, new CreateBookingDto { GymClassId = id });
            return CreatedAtAction(nameof(GetBookings), new { id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Cancel a booking. The member who booked it, or Admin.</summary>
    [HttpDelete("{id:int}/bookings/{bookingId:int}")]
    public async Task<IActionResult> CancelBooking(int id, int bookingId)
    {
        var memberId = JwtHelper.GetUserId(User);
        var cancelled = await _classService.CancelBookingAsync(memberId, bookingId);
        return cancelled ? NoContent() : NotFound();
    }
}
