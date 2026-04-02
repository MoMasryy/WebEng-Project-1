using System.ComponentModel.DataAnnotations;

namespace GymPulse.DTOs.Requests;

public class CreateSubscriptionDto
{
    [Required, MaxLength(50)]
    public string Plan { get; set; } = string.Empty;  // Basic | Premium | VIP

    [Range(0.01, 9999.99)]
    public decimal PricePerMonth { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class CreateBookingDto
{
    [Required]
    public int GymClassId { get; set; }
}

public class UpdateMemberProfileDto
{
    [MaxLength(300)]
    public string? Bio { get; set; }

    [MaxLength(100)]
    public string? FitnessGoal { get; set; }

    [Range(20, 400)]
    public double WeightKg { get; set; }

    [Range(50, 280)]
    public double HeightCm { get; set; }
}
