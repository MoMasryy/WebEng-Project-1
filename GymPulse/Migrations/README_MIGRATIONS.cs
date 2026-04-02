/*
 * ─────────────────────────────────────────────────────────────────────────────
 * HOW TO CREATE AND APPLY EF CORE MIGRATIONS
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * STEP 1 — Make sure your connection string in appsettings.json is correct.
 *
 * STEP 2 — In your terminal (inside the GymPulse project folder), run:
 *
 *    dotnet ef migrations add InitialCreate
 *
 *   This generates migration files in the /Migrations folder automatically.
 *   EF Core reads your AppDbContext and models to create the SQL schema.
 *
 * STEP 3 — Apply the migration to create the database:
 *
 *    dotnet ef database update
 *
 *   This runs the SQL against your SQL Server and creates all tables.
 *
 * STEP 4 — If you change a model later, repeat:
 *
 *    dotnet ef migrations add <DescriptiveName>
 *    dotnet ef database update
 *
 * ─────────────────────────────────────────────────────────────────────────────
 * TABLES CREATED:
 *   - Members          (Id, FullName, Email, PasswordHash, Phone, Role, ...)
 *   - MemberProfiles   (Id, MemberId FK, Bio, FitnessGoal, WeightKg, ...)
 *   - Trainers         (Id, FullName, Email, PasswordHash, Specialty, ...)
 *   - GymClasses       (Id, Title, ScheduledAt, TrainerId FK, ...)
 *   - ClassBookings    (Id, MemberId FK, GymClassId FK, Status, ...)
 *   - Subscriptions    (Id, MemberId FK, Plan, PricePerMonth, ...)
 * ─────────────────────────────────────────────────────────────────────────────
 */
