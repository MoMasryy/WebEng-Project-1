using GymPulse.DTOs.Requests;
using GymPulse.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymPulse.Controllers;

/// <summary>
/// Manages trainer accounts. Only Admins can create or delete trainers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _trainerService;

    public TrainersController(ITrainerService trainerService) =>
        _trainerService = trainerService;

    /// <summary>Get all trainers. Any authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _trainerService.GetAllAsync());

    /// <summary>Get a trainer by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _trainerService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new trainer account. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTrainerDto dto)
    {
        var result = await _trainerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update trainer info. Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTrainerDto dto)
    {
        var result = await _trainerService.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a trainer. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _trainerService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
