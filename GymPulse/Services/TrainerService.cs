using GymPulse.Data;
using GymPulse.DTOs.Requests;
using GymPulse.DTOs.Responses;
using GymPulse.Helpers;
using GymPulse.Models;
using GymPulse.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Services;

public class TrainerService : ITrainerService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public TrainerService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<IEnumerable<TrainerResponse>> GetAllAsync()
    {
        return await _db.Trainers
            .AsNoTracking()
            .Select(t => new TrainerResponse(
                t.Id,
                t.FullName,
                t.Email,
                t.Specialty,
                t.IsAvailable,
                t.Classes.Count(c => c.ScheduledAt >= DateTime.UtcNow)))
            .ToListAsync();
    }

    public async Task<TrainerResponse?> GetByIdAsync(int id)
    {
        return await _db.Trainers
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TrainerResponse(
                t.Id,
                t.FullName,
                t.Email,
                t.Specialty,
                t.IsAvailable,
                t.Classes.Count(c => c.ScheduledAt >= DateTime.UtcNow)))
            .FirstOrDefaultAsync();
    }

    public async Task<TrainerResponse> CreateAsync(CreateTrainerDto dto)
    {
        var trainer = new Trainer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Specialty = dto.Specialty,
            Role = "Trainer"
        };

        _db.Trainers.Add(trainer);
        await _db.SaveChangesAsync();

        return new TrainerResponse(trainer.Id, trainer.FullName,
            trainer.Email, trainer.Specialty, trainer.IsAvailable, 0);
    }

    public async Task<TrainerResponse?> UpdateAsync(int id, UpdateTrainerDto dto)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer is null) return null;

        if (dto.FullName is not null) trainer.FullName = dto.FullName;
        if (dto.Specialty is not null) trainer.Specialty = dto.Specialty;
        if (dto.IsAvailable.HasValue) trainer.IsAvailable = dto.IsAvailable.Value;

        await _db.SaveChangesAsync();

        return new TrainerResponse(trainer.Id, trainer.FullName,
            trainer.Email, trainer.Specialty, trainer.IsAvailable, 0);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var trainer = await _db.Trainers.FindAsync(id);
        if (trainer is null) return false;

        _db.Trainers.Remove(trainer);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<AuthResponse?> LoginAsync(LoginDto dto)
    {
        var trainer = await _db.Trainers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Email == dto.Email);

        if (trainer is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, trainer.PasswordHash)) return null;

        var token = JwtHelper.GenerateToken(_config, trainer.Id, trainer.Email, trainer.Role);
        return new AuthResponse(token, trainer.Role, trainer.FullName);
    }
}
