using GymPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberProfile> MemberProfiles => Set<MemberProfile>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<GymClass> GymClasses => Set<GymClass>();
    public DbSet<ClassBooking> ClassBookings => Set<ClassBooking>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ─── One-to-One: Member ↔ MemberProfile ──────────────────────────────
        modelBuilder.Entity<Member>()
            .HasOne(m => m.Profile)
            .WithOne(p => p.Member)
            .HasForeignKey<MemberProfile>(p => p.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── One-to-Many: Trainer → GymClass ─────────────────────────────────
        modelBuilder.Entity<GymClass>()
            .HasOne(c => c.Trainer)
            .WithMany(t => t.Classes)
            .HasForeignKey(c => c.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ─── One-to-Many: Member → Subscription ──────────────────────────────
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Member)
            .WithMany(m => m.Subscriptions)
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Many-to-Many: Member ↔ GymClass via ClassBooking ────────────────
        modelBuilder.Entity<ClassBooking>()
            .HasOne(b => b.Member)
            .WithMany(m => m.ClassBookings)
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClassBooking>()
            .HasOne(b => b.GymClass)
            .WithMany(c => c.ClassBookings)
            .HasForeignKey(b => b.GymClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index: one booking per member per class
        modelBuilder.Entity<ClassBooking>()
            .HasIndex(b => new { b.MemberId, b.GymClassId })
            .IsUnique();

        // Decimal precision
        modelBuilder.Entity<Subscription>()
            .Property(s => s.PricePerMonth)
            .HasColumnType("TEXT");
    }
}
