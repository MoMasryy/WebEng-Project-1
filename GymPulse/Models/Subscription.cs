namespace GymPulse.Models;

public class Subscription
{
    public int Id { get; set; }
    public int MemberId { get; set; }                   // FK - one-to-many from Member
    public string Plan { get; set; } = string.Empty;    // Basic | Premium | VIP
    public decimal PricePerMonth { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Member Member { get; set; } = null!;
}
