namespace GymPulse.Models;

public class Trainer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;  // e.g. "CrossFit", "Yoga"
    public string Role { get; set; } = "Trainer";
    public bool IsAvailable { get; set; } = true;
    public DateTime HiredAt { get; set; } = DateTime.UtcNow;

    // One-to-Many: Trainer can run many GymClasses
    public ICollection<GymClass> Classes { get; set; } = new List<GymClass>();
}
