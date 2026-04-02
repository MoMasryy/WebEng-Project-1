using GymPulse.DTOs.Requests;
using GymPulse.DTOs.Responses;

namespace GymPulse.Services.Interfaces;

public interface ITrainerService
{
    Task<IEnumerable<TrainerResponse>> GetAllAsync();
    Task<TrainerResponse?> GetByIdAsync(int id);
    Task<TrainerResponse> CreateAsync(CreateTrainerDto dto);
    Task<TrainerResponse?> UpdateAsync(int id, UpdateTrainerDto dto);
    Task<bool> DeleteAsync(int id);
    Task<AuthResponse?> LoginAsync(LoginDto dto);
}

public interface IGymClassService
{
    Task<IEnumerable<GymClassResponse>> GetAllAsync();
    Task<GymClassResponse?> GetByIdAsync(int id);
    Task<GymClassResponse> CreateAsync(CreateGymClassDto dto);
    Task<GymClassResponse?> UpdateAsync(int id, UpdateGymClassDto dto);
    Task<bool> DeleteAsync(int id);
    Task<BookingResponse> BookAsync(int memberId, CreateBookingDto dto);
    Task<bool> CancelBookingAsync(int memberId, int bookingId);
    Task<IEnumerable<BookingResponse>> GetBookingsForClassAsync(int classId);
}
