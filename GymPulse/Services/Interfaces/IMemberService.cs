using GymPulse.DTOs.Requests;
using GymPulse.DTOs.Responses;

namespace GymPulse.Services.Interfaces;

public interface IMemberService
{
    Task<MemberResponse?> GetByIdAsync(int id);
    Task<IEnumerable<MemberResponse>> GetAllAsync();
    Task<MemberResponse> RegisterAsync(RegisterMemberDto dto);
    Task<MemberResponse?> UpdateAsync(int id, UpdateMemberDto dto);
    Task<bool> DeleteAsync(int id);
    Task<MemberProfileResponse?> GetProfileAsync(int memberId);
    Task<MemberProfileResponse> UpsertProfileAsync(int memberId, UpdateMemberProfileDto dto);
    Task<AuthResponse?> LoginAsync(LoginDto dto);
}
