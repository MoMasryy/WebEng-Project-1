GymPulse
Gym Management System API
ASP.NET Core Web API  |  Entity Framework Core  |  JWT Auth
Web Engineering Assignment  —  2025


1. Project Overview
GymPulse is a fully-featured REST API for managing a gym. It was built with ASP.NET Core 8 and Entity Framework Core using a clean layered architecture: Models → Services → Controllers. The system tracks trainers, members, gym classes, bookings, and subscriptions, all secured with JWT authentication and role-based authorization.

The API demonstrates all three required relationship types:
•One-to-One: Each Member has exactly one MemberProfile (fitness stats).
•One-to-Many: Each Trainer runs many GymClasses. Each Member has many Subscriptions.
•Many-to-Many: Members can book many GymClasses, and each class can have many Members, linked via the ClassBooking join table.

2. Technologies Used

Technology	Version	Purpose
ASP.NET Core	8.0	Web API framework — handles HTTP routing, middleware, DI container
Entity Framework Core	8.0	ORM — maps C# models to SQL tables, manages migrations
SQL Server (LocalDB)	2022	Relational database storing all gym data persistently
Microsoft.AspNetCore.Authentication.JwtBearer	8.0	Validates JWT tokens on every protected request
System.IdentityModel.Tokens.Jwt	7.3	Creates and signs JWT tokens during login
BCrypt.Net-Next	4.0	Hashes passwords before storage — never plain text
Swashbuckle (Swagger)	6.5	Auto-generates interactive API docs at /swagger
Hangfire	1.8	Schedules background jobs (cron) without a separate process

3. How to Run the Project
Prerequisites
•Install .NET 8 SDK from https://dotnet.microsoft.com/download
•Install SQL Server (Developer Edition) or SQL Server LocalDB
•Install Visual Studio 2022 or VS Code with the C# extension

Step-by-Step Setup
Step 1 — Clone or unzip the project
cd GymPulse

Step 2 — Configure the database connection
Open appsettings.json and update the connection string if needed:
"DefaultConnection": "Server=localhost;Database=GymPulseDb;Trusted_Connection=True;TrustServerCertificate=True;"

Step 3 — Create the database with EF Core migrations
Run these two commands in the terminal inside the project folder:
dotnet ef migrations add InitialCreate
dotnet ef database update
This creates all tables in SQL Server automatically based on the models.

Step 4 — Run the API
dotnet run
The API starts at https://localhost:5001 (or http://localhost:5000).

Step 5 — Open Swagger
Navigate to https://localhost:5001/swagger in your browser to see all endpoints and test them interactively.

4. API Endpoint Documentation

Authentication — /api/auth
Method	Endpoint	Auth Required	Description
POST	/api/auth/register	No	Register a new member account
POST	/api/auth/login/member	No	Login as member, returns JWT token
POST	/api/auth/login/trainer	No	Login as trainer, returns JWT token

Members — /api/members
Method	Endpoint	Role	Description
GET	/api/members	Admin	List all members
GET	/api/members/{id}	Admin / Self	Get a member by ID
PUT	/api/members/{id}	Admin / Self	Update name or phone
DELETE	/api/members/{id}	Admin	Delete a member
GET	/api/members/{id}/profile	Authenticated	Get fitness profile (1-to-1)
PUT	/api/members/{id}/profile	Admin / Self	Create or update fitness profile
GET	/api/members/{id}/subscriptions	Authenticated	List subscriptions (1-to-many)
POST	/api/members/{id}/subscriptions	Admin	Add a new subscription
DELETE	/api/members/{id}/subscriptions/{sid}	Admin	Cancel a subscription

Trainers — /api/trainers
Method	Endpoint	Role	Description
GET	/api/trainers	Authenticated	List all trainers
GET	/api/trainers/{id}	Authenticated	Get trainer by ID
POST	/api/trainers	Admin	Create a trainer account
PUT	/api/trainers/{id}	Admin	Update trainer info
DELETE	/api/trainers/{id}	Admin	Delete a trainer

Gym Classes — /api/gymclasses
Method	Endpoint	Role	Description
GET	/api/gymclasses	Authenticated	List all classes
GET	/api/gymclasses/{id}	Authenticated	Get class by ID
POST	/api/gymclasses	Admin / Trainer	Create a new class
PUT	/api/gymclasses/{id}	Admin / Trainer	Update class details
DELETE	/api/gymclasses/{id}	Admin	Delete a class
GET	/api/gymclasses/{id}/bookings	Admin / Trainer	Get all bookings for a class
POST	/api/gymclasses/{id}/book	Member	Book a class (many-to-many)
DELETE	/api/gymclasses/{id}/bookings/{bid}	Member / Admin	Cancel a booking

5. Architecture & Design Decisions
Layered Architecture
The project follows a clean 3-layer architecture so each concern is separated:
•Models layer — pure C# classes representing database tables. No logic here.
•Service layer — all business logic lives here. Services depend on AppDbContext via constructor injection. Controllers never touch the database directly.
•Controller layer — thin layer that validates HTTP input, calls a service, and returns an HTTP response. Nothing else.

Dependency Injection
Services are registered in Program.cs using AddScoped<Interface, Implementation>(). This means ASP.NET Core creates one instance per HTTP request and automatically injects it into any controller or other service that needs it. You never call new MemberService() yourself.

DTOs (Data Transfer Objects)
Controllers never return Entity models directly. Instead, dedicated DTO classes control exactly what data is exposed. There are three categories:
•Request DTOs — validate incoming data using Data Annotations ([Required], [MaxLength], etc.)
•Response DTOs — shape the output, hiding sensitive fields like PasswordHash
•Automatic 400 validation — because the API uses [ApiController], any invalid request is rejected with HTTP 400 before the controller method even runs

LINQ Optimization
All read queries use two key optimizations:
•AsNoTracking() — tells EF Core not to track changes on these entities, reducing memory usage and improving read speed since we have no intention of saving changes.
•Select() projections — instead of loading the entire entity into memory and then converting it, the Select() runs the transformation inside the SQL query itself, so only the needed columns travel over the network.

6. Security & Authentication
How JWT Authentication Works
1. The client sends email + password to POST /api/auth/login/member.
2. The server verifies the password using BCrypt.Verify() against the stored hash.
3. If valid, a JWT token is generated containing the user's ID, email, and role, signed with HMAC-SHA256 using a secret key.
4. The client stores the token and sends it with every subsequent request in the Authorization header: Bearer <token>.
5. ASP.NET Core's JwtBearer middleware automatically validates the token signature, expiry, issuer, and audience on every request before it reaches a controller.

Role-Based Authorization
Three roles exist: Admin, Trainer, and Member. The [Authorize(Roles = "Admin")] attribute restricts specific endpoints. Role is stored inside the JWT claims, so the server never needs to look it up from the database on each request.

Why HTTP-Only Cookies Are the Industry Standard for Authentication
When a web application needs to store a JWT token on the client side, there are two main options: localStorage/sessionStorage or HTTP-only cookies. The industry strongly prefers HTTP-only cookies for the following reasons:

•Protection against XSS (Cross-Site Scripting): Tokens stored in localStorage are accessible to any JavaScript on the page. If an attacker injects a malicious script (via XSS), they can steal the token and hijack the session. HTTP-only cookies cannot be read by JavaScript at all — the browser only sends them automatically with HTTP requests, making XSS token theft impossible.

•Automatic transmission: The browser automatically attaches HTTP-only cookies to every request to the matching domain, removing the need for client-side code to manually read and attach the token.

•Secure flag: HTTP-only cookies can be combined with the Secure flag, ensuring they are only ever sent over HTTPS connections, preventing interception on unencrypted networks.

•SameSite attribute: Setting SameSite=Strict or SameSite=Lax on the cookie protects against CSRF (Cross-Site Request Forgery) attacks, where a malicious site tricks the user's browser into making authenticated requests.

In summary, HTTP-only cookies provide defense-in-depth: they protect tokens from JavaScript injection attacks, enforce HTTPS transmission, and offer CSRF mitigation — making them far more secure than storing tokens in browser storage.

7. Background Jobs (Hangfire — Bonus)
Two recurring jobs run automatically in the background using Hangfire:

Job Name	Schedule	What It Does
DeactivateExpiredSubscriptions	Daily (midnight)	Finds all subscriptions where EndDate has passed and sets IsActive = false automatically.
MarkAttendedBookings	Hourly	Finds all 'Confirmed' bookings for classes that ran more than 24 hours ago and marks them as 'Attended'.

The Hangfire dashboard is available at /hangfire during development, showing all scheduled jobs, their last run times, and any errors.

8. Database Schema

Table	Key Columns	Relationship
Members	Id, FullName, Email, PasswordHash, Role	Has one Profile, many Subscriptions, many Bookings
MemberProfiles	Id, MemberId (FK), Bio, FitnessGoal, WeightKg	One-to-One with Members
Trainers	Id, FullName, Email, PasswordHash, Specialty	Has many GymClasses
GymClasses	Id, Title, ScheduledAt, MaxCapacity, TrainerId (FK)	Belongs to one Trainer, has many Bookings
ClassBookings	Id, MemberId (FK), GymClassId (FK), Status	Join table — Many-to-Many between Members and GymClasses
Subscriptions	Id, MemberId (FK), Plan, PricePerMonth, IsActive	One-to-Many from Members
