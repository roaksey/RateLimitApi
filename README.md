📘 RateLimitApi

A .NET 8 Web API + SQL Server project that provides:

Rate limiting by API key + IP address

Different request limits for Free / Basic / Pro subscription tiers

Abuse detection (short bursts, rapid key switching, suspicious patterns)

Temporary blocking of abusive users/IPs

Logging of all requests and breaches

Admin API to list/unblock users

Alerts via webhook or email when users approach daily limits

🚀 Getting Started
1. Prerequisites

Visual Studio 2022
 with ASP.NET and web development workload

.NET 8 SDK

SQL Server (Developer or Express edition works fine)

SQL Server Management Studio (SSMS) or Azure Data Studio (optional but recommended)

2. Clone the Repo
git clone https://github.com/YOURNAME/RateLimitApi.git
cd RateLimitApi

3. Create Database

Open SQL Server Management Studio and run the included script:

-- Run Database.sql


Or from terminal:

sqlcmd -S localhost -i Database.sql


This will create a database named RateLimitDemo, tables, indexes, and seed three API keys:

FREE-KEY-1 → Free (100 requests/day)

BASIC-KEY-1 → Basic (1000 requests/day)

PRO-KEY-1 → Pro (unlimited)

4. Configure App Settings

Edit appsettings.json and update your SQL connection string:

"ConnectionStrings": {
  "Sql": "Server=localhost;Database=RateLimitDemo;Trusted_Connection=True;TrustServerCertificate=True"
}


If running SQL Server on another host or with SQL authentication:

"Sql": "Server=YOURSERVER;Database=RateLimitDemo;User Id=sa;Password=YourPassword;TrustServerCertificate=True"


Optional: configure alerts (WebhookUrl or SMTP settings).

5. Run the Application

From Visual Studio 2022:

Open RateLimitApi.csproj

Press F5 (or Ctrl+F5)

From CLI:

dotnet run

6. Test the API

Swagger UI:

https://localhost:5001/swagger

Example request:
curl -H "X-Api-Key: FREE-KEY-1" https://localhost:5001/api/data

Admin API:

List blocks:

GET https://localhost:5001/admin/blocks


Unblock:

POST https://localhost:5001/admin/unblock
{
  "apiKey": "FREE-KEY-1",
  "ip": "127.0.0.1"
}

🗂 Project Structure
RateLimitApi/
│   RateLimitApi.csproj      → project file
│   Program.cs               → entry point
│   appsettings.json         → config (SQL, alerts, abuse thresholds)
│   Database.sql             → SQL schema + seed data
│
├── Controllers/             → API endpoints
│   DataController.cs        → protected demo endpoint
│   AdminController.cs       → admin endpoints (list/unblock)
│
├── Data/                    
│   SqlRepository.cs         → SQL operations
│   Models.cs                → DTOs
│
├── Services/
│   RateLimitService.cs      → main rate-limit logic
│   AbuseDetectionService.cs → abuse detection rules
│   AlertService.cs          → alerts (webhook/email)
│
└── Config/
    AlertConfig.cs           → alert config loader
    AbuseConfig.cs           → abuse thresholds

✅ Features Recap

Rate Limits

Free → 100/day

Basic → 1000/day

Pro → Unlimited

Abuse Detection

Short bursts → temporary block

Rapid API key switching → IP block

(Optional) automation patterns

Admin Tools

List active blocks

Unblock users or IPs

Alerts

Webhook (JSON POST)

Email (SMTP)

🛠 Next Steps

Protect /admin/* routes with authentication (API key, JWT, or IP allowlist).

Replace direct SQL queries with EF Core if preferred.

Extend alerts to Slack, Teams, or monitoring systems.
