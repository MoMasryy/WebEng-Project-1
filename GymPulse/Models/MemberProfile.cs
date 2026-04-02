namespace GymPulse.Models;

public class MemberProfile
{
    public int Id { get; set; }
    public int MemberId { get; set; }           // FK to Member
    public string? Bio { get; set; }
    public string? FitnessGoal { get; set; }    // e.g. "Weight Loss", "Muscle Gain"
    public double WeightKg { get; set; }
    public double HeightCm { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation back to Member
    public Member Member { get; set; } = null!;
}
