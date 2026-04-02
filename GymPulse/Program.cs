using System.Text;
using GymPulse.Data;
using GymPulse.Jobs;
using GymPulse.Middleware;
using GymPulse.Services;
using GymPulse.Services.Interfaces;
using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database ───────────────────────────────────────────────────────────────
// We use SQL Server with Entity Framework Core.
// The connection string lives in appsettings.json so credentials are never in code.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. Services (Dependency Injection) ───────────────────────────────────────
// Registering as Scoped means one instance per HTTP request — correct for DB work.
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<IGymClassService, GymClassService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<GymMaintenanceJobs>();

// ── 3. JWT Authentication ─────────────────────────────────────────────────────
// Clients must send: Authorization: Bearer <token>
var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ── 4. Hangfire Background Jobs ───────────────────────────────────────────────
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer();

// ── 5. Swagger (API Documentation) ───────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GymPulse API",
        Version = "v1",
        Description = "REST API for the GymPulse Gym Management System"
    });

    // Allow Swagger to send JWT tokens via the Authorize button
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here. Example: eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── 6. Middleware Pipeline ────────────────────────────────────────────────────
// Order matters! Exception handler must be first so it wraps everything else.
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger always enabled for easy testing
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymPulse API v1"));

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// ── 7. Hangfire Dashboard ─────────────────────────────────────────────────────
// Visit /hangfire in your browser to see scheduled jobs
app.UseHangfireDashboard("/hangfire");

// Schedule recurring jobs using cron expressions
RecurringJob.AddOrUpdate<GymMaintenanceJobs>(
    "deactivate-expired-subscriptions",
    job => job.DeactivateExpiredSubscriptionsAsync(),
    Cron.Daily);  // Every day at midnight

RecurringJob.AddOrUpdate<GymMaintenanceJobs>(
    "mark-attended-bookings",
    job => job.MarkAttendedBookingsAsync(),
    Cron.Hourly); // Every hour

app.MapControllers();

app.Run();
