namespace GymPulse.DTOs.Responses;

public record MemberResponse(
    int Id,
    string FullName,
    string Email,
    string Phone,
    string Role,
    DateTime CreatedAt
);

public record MemberProfileResponse(
    int MemberId,
    string? Bio,
    string? FitnessGoal,
    double WeightKg,
    double HeightCm,
    DateTime LastUpdated
);

public record TrainerResponse(
    int Id,
    string FullName,
    string Email,
    string Specialty,
    bool IsAvailable,
    int ActiveClassCount
);

public record GymClassResponse(
    int Id,
    string Title,
    string Description,
    DateTime ScheduledAt,
    int DurationMinutes,
    int MaxCapacity,
    int BookedCount,
    string TrainerName
);

public record SubscriptionResponse(
    int Id,
    string Plan,
    decimal PricePerMonth,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);

public record BookingResponse(
    int Id,
    string MemberName,
    string ClassTitle,
    DateTime BookedAt,
    string Status
);

public record AuthResponse(
    string Token,
    string Role,
    string FullName
);
