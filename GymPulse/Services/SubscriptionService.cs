using GymPulse.Data;
using GymPulse.DTOs.Requests;
using GymPulse.DTOs.Responses;
using GymPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Services;

public interface ISubscriptionService
{
    Task<IEnumerable<SubscriptionResponse>> GetByMemberAsync(int memberId);
    Task<SubscriptionResponse> CreateAsync(int memberId, CreateSubscriptionDto dto);
    Task<bool> CancelAsync(int memberId, int subscriptionId);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;

    public SubscriptionService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<SubscriptionResponse>> GetByMemberAsync(int memberId)
    {
        return await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.MemberId == memberId)
            .Select(s => new SubscriptionResponse(
                s.Id, s.Plan, s.PricePerMonth,
                s.StartDate, s.EndDate, s.IsActive))
            .ToListAsync();
    }

    public async Task<SubscriptionResponse> CreateAsync(int memberId, CreateSubscriptionDto dto)
    {
        if (dto.EndDate <= dto.StartDate)
            throw new InvalidOperationException("EndDate must be after StartDate.");

        var subscription = new Subscription
        {
            MemberId = memberId,
            Plan = dto.Plan,
            PricePerMonth = dto.PricePerMonth,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = true
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync();

        return new SubscriptionResponse(subscription.Id, subscription.Plan,
            subscription.PricePerMonth, subscription.StartDate,
            subscription.EndDate, subscription.IsActive);
    }

    public async Task<bool> CancelAsync(int memberId, int subscriptionId)
    {
        var sub = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.MemberId == memberId);

        if (sub is null) return false;

        sub.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
