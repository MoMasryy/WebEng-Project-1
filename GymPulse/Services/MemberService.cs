using GymPulse.Data;
using GymPulse.DTOs.Requests;
using GymPulse.DTOs.Responses;
using GymPulse.Helpers;
using GymPulse.Models;
using GymPulse.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    // Dependency injection: AppDbContext and IConfiguration are injected by ASP.NET Core
    public MemberService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<IEnumerable<MemberResponse>> GetAllAsync()
    {
        // AsNoTracking() = we only read, EF doesn't track changes → faster query
        // Select() = project only the fields we need, not the entire entity
        return await _db.Members
            .AsNoTracking()
            .Select(m => new MemberResponse(
                m.Id, m.FullName, m.Email, m.Phone, m.Role, m.CreatedAt))
            .ToListAsync();
    }

    public async Task<MemberResponse?> GetByIdAsync(int id)
    {
        return await _db.Members
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MemberResponse(
                m.Id, m.FullName, m.Email, m.Phone, m.Role, m.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<MemberResponse> RegisterAsync(RegisterMemberDto dto)
    {
        // Hash the password before storing — never store plain text passwords
        var member = new Member
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Role = "Member"
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        return new MemberResponse(
            member.Id, member.FullName, member.Email,
            member.Phone, member.Role, member.CreatedAt);
    }

    public async Task<MemberResponse?> UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _db.Members.FindAsync(id);
        if (member is null) return null;

        if (dto.FullName is not null) member.FullName = dto.FullName;
        if (dto.Phone is not null) member.Phone = dto.Phone;

        await _db.SaveChangesAsync();

        return new MemberResponse(
            member.Id, member.FullName, member.Email,
            member.Phone, member.Role, member.CreatedAt);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member is null) return false;

        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<MemberProfileResponse?> GetProfileAsync(int memberId)
    {
        return await _db.MemberProfiles
            .AsNoTracking()
            .Where(p => p.MemberId == memberId)
            .Select(p => new MemberProfileResponse(
                p.MemberId, p.Bio, p.FitnessGoal,
                p.WeightKg, p.HeightCm, p.LastUpdated))
            .FirstOrDefaultAsync();
    }

    public async Task<MemberProfileResponse> UpsertProfileAsync(int memberId, UpdateMemberProfileDto dto)
    {
        var profile = await _db.MemberProfiles
            .FirstOrDefaultAsync(p => p.MemberId == memberId);

        if (profile is null)
        {
            profile = new MemberProfile { MemberId = memberId };
            _db.MemberProfiles.Add(profile);
        }

        profile.Bio = dto.Bio;
        profile.FitnessGoal = dto.FitnessGoal;
        profile.WeightKg = dto.WeightKg;
        profile.HeightCm = dto.HeightCm;
        profile.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new MemberProfileResponse(
            profile.MemberId, profile.Bio, profile.FitnessGoal,
            profile.WeightKg, profile.HeightCm, profile.LastUpdated);
    }

    public async Task<AuthResponse?> LoginAsync(LoginDto dto)
    {
        var member = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Email == dto.Email);

        if (member is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, member.PasswordHash)) return null;

        var token = JwtHelper.GenerateToken(_config, member.Id, member.Email, member.Role);
        return new AuthResponse(token, member.Role, member.FullName);
    }
}
