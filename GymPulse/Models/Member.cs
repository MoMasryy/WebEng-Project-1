namespace GymPulse.Models;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Role { get; set; } = "Member"; // Member | Admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // One-to-One: each Member has one MemberProfile
    public MemberProfile? Profile { get; set; }

    // One-to-Many: a Member can have many Subscriptions
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    // Many-to-Many: a Member can attend many Classes
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
}
