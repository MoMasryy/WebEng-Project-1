namespace GymPulse.Models;

public class GymClass
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;           // e.g. "Morning HIIT"
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxCapacity { get; set; }
    public int TrainerId { get; set; }   // FK - one-to-many from Trainer

    // Navigation
    public Trainer Trainer { get; set; } = null!;

    // Many-to-Many: Class can have many Members (via ClassBooking)
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
}

/// <summary>
/// Join table for the Many-to-Many between Member and GymClass.
/// Using an explicit join entity so we can store extra data (e.g. status).
/// </summary>
public class ClassBooking
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int GymClassId { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed"; // Confirmed | Cancelled | Attended

    public Member Member { get; set; } = null!;
    public GymClass GymClass { get; set; } = null!;
}
