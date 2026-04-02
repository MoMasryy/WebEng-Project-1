using System.ComponentModel.DataAnnotations;

namespace GymPulse.DTOs.Requests;

public class CreateTrainerDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
}

public class UpdateTrainerDto
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(100)]
    public string? Specialty { get; set; }

    public bool? IsAvailable { get; set; }
}
