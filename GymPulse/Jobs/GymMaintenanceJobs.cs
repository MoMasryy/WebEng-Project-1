using GymPulse.Data;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Jobs;

/// <summary>
/// Background jobs scheduled with Hangfire.
/// These run automatically on a cron schedule — no HTTP request needed.
/// </summary>
public class GymMaintenanceJobs
{
    private readonly AppDbContext _db;
    private readonly ILogger<GymMaintenanceJobs> _logger;

    public GymMaintenanceJobs(AppDbContext db, ILogger<GymMaintenanceJobs> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Runs every day at midnight.
    /// Automatically marks subscriptions as inactive if their EndDate has passed.
    /// </summary>
    public async Task DeactivateExpiredSubscriptionsAsync()
    {
        var expired = await _db.Subscriptions
            .Where(s => s.IsActive && s.EndDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var sub in expired)
            sub.IsActive = false;

        await _db.SaveChangesAsync();
        _logger.LogInformation("[GymPulse] Deactivated {Count} expired subscriptions.", expired.Count);
    }

    /// <summary>
    /// Runs every hour.
    /// Cleans up "Confirmed" bookings for classes that happened more than 24 hours ago
    /// by marking them as "Attended" to keep the data clean.
    /// </summary>
    public async Task MarkAttendedBookingsAsync()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var oldBookings = await _db.ClassBookings
            .Include(b => b.GymClass)
            .Where(b => b.Status == "Confirmed" && b.GymClass.ScheduledAt < cutoff)
            .ToListAsync();

        foreach (var booking in oldBookings)
            booking.Status = "Attended";

        await _db.SaveChangesAsync();
        _logger.LogInformation("[GymPulse] Marked {Count} bookings as Attended.", oldBookings.Count);
    }
}
