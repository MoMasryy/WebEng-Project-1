using GymPulse.Data;
using GymPulse.DTOs.Requests;
using GymPulse.DTOs.Responses;
using GymPulse.Models;
using GymPulse.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Services;

public class GymClassService : IGymClassService
{
    private readonly AppDbContext _db;

    public GymClassService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<GymClassResponse>> GetAllAsync()
    {
        return await _db.GymClasses
            .AsNoTracking()
            .Select(c => new GymClassResponse(
                c.Id,
                c.Title,
                c.Description,
                c.ScheduledAt,
                c.DurationMinutes,
                c.MaxCapacity,
                c.ClassBookings.Count(b => b.Status == "Confirmed"),
                c.Trainer.FullName))
            .ToListAsync();
    }

    public async Task<GymClassResponse?> GetByIdAsync(int id)
    {
        return await _db.GymClasses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new GymClassResponse(
                c.Id,
                c.Title,
                c.Description,
                c.ScheduledAt,
                c.DurationMinutes,
                c.MaxCapacity,
                c.ClassBookings.Count(b => b.Status == "Confirmed"),
                c.Trainer.FullName))
            .FirstOrDefaultAsync();
    }

    public async Task<GymClassResponse> CreateAsync(CreateGymClassDto dto)
    {
        var gymClass = new GymClass
        {
            Title = dto.Title,
            Description = dto.Description,
            ScheduledAt = dto.ScheduledAt,
            DurationMinutes = dto.DurationMinutes,
            MaxCapacity = dto.MaxCapacity,
            TrainerId = dto.TrainerId
        };

        _db.GymClasses.Add(gymClass);
        await _db.SaveChangesAsync();

        // Reload with trainer name for the response
        var trainerName = await _db.Trainers
            .AsNoTracking()
            .Where(t => t.Id == dto.TrainerId)
            .Select(t => t.FullName)
            .FirstOrDefaultAsync() ?? "Unknown";

        return new GymClassResponse(gymClass.Id, gymClass.Title, gymClass.Description,
            gymClass.ScheduledAt, gymClass.DurationMinutes, gymClass.MaxCapacity, 0, trainerName);
    }

    public async Task<GymClassResponse?> UpdateAsync(int id, UpdateGymClassDto dto)
    {
        var gymClass = await _db.GymClasses.FindAsync(id);
        if (gymClass is null) return null;

        if (dto.Title is not null) gymClass.Title = dto.Title;
        if (dto.Description is not null) gymClass.Description = dto.Description;
        if (dto.ScheduledAt.HasValue) gymClass.ScheduledAt = dto.ScheduledAt.Value;
        if (dto.DurationMinutes.HasValue) gymClass.DurationMinutes = dto.DurationMinutes.Value;
        if (dto.MaxCapacity.HasValue) gymClass.MaxCapacity = dto.MaxCapacity.Value;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var gymClass = await _db.GymClasses.FindAsync(id);
        if (gymClass is null) return false;

        _db.GymClasses.Remove(gymClass);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<BookingResponse> BookAsync(int memberId, CreateBookingDto dto)
    {
        // Check class exists and has capacity
        var gymClass = await _db.GymClasses
            .Include(c => c.ClassBookings)
            .FirstOrDefaultAsync(c => c.Id == dto.GymClassId)
            ?? throw new InvalidOperationException("Class not found.");

        var confirmedCount = gymClass.ClassBookings.Count(b => b.Status == "Confirmed");
        if (confirmedCount >= gymClass.MaxCapacity)
            throw new InvalidOperationException("Class is fully booked.");

        // Check member hasn't already booked this class
        var duplicate = await _db.ClassBookings
            .AnyAsync(b => b.MemberId == memberId && b.GymClassId == dto.GymClassId);
        if (duplicate)
            throw new InvalidOperationException("Already booked this class.");

        var booking = new ClassBooking
        {
            MemberId = memberId,
            GymClassId = dto.GymClassId,
            Status = "Confirmed"
        };

        _db.ClassBookings.Add(booking);
        await _db.SaveChangesAsync();

        var memberName = await _db.Members
            .AsNoTracking()
            .Where(m => m.Id == memberId)
            .Select(m => m.FullName)
            .FirstOrDefaultAsync() ?? "Unknown";

        return new BookingResponse(
            booking.Id, memberName, gymClass.Title,
            booking.BookedAt, booking.Status);
    }

    public async Task<bool> CancelBookingAsync(int memberId, int bookingId)
    {
        var booking = await _db.ClassBookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.MemberId == memberId);

        if (booking is null) return false;

        booking.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BookingResponse>> GetBookingsForClassAsync(int classId)
    {
        return await _db.ClassBookings
            .AsNoTracking()
            .Where(b => b.GymClassId == classId)
            .Select(b => new BookingResponse(
                b.Id,
                b.Member.FullName,
                b.GymClass.Title,
                b.BookedAt,
                b.Status))
            .ToListAsync();
    }
}
