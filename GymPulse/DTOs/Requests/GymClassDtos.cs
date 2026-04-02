using System.ComponentModel.DataAnnotations;

namespace GymPulse.DTOs.Requests;

public class CreateGymClassDto
{
    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Range(15, 180)]
    public int DurationMinutes { get; set; }

    [Range(1, 100)]
    public int MaxCapacity { get; set; }

    [Required]
    public int TrainerId { get; set; }
}

public class UpdateGymClassDto
{
    [MaxLength(120)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? ScheduledAt { get; set; }

    [Range(15, 180)]
    public int? DurationMinutes { get; set; }

    [Range(1, 100)]
    public int? MaxCapacity { get; set; }
}
