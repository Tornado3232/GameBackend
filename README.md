# Backend & Data Engineering Assignment
(.NET 10 + Python 3.13 • Docker Compose)

This repository implements the Backend & Data Engineering technical assignment using .NET 10 for the game backend API and Python 3.13 for the data reliability tasks.
The entire system runs using Docker Compose, following the requirements defined in the project brief.


------------------------------------------------------------
1. Project Overview
------------------------------------------------------------

1.1 Core Game Backend API (.NET 10)

Endpoint                 | Description
-------------------------|----------------------------------------
POST /auth/register      | Register a user
POST /auth/login         | User login and return JWT
POST /earn/earn          | Increase user balance and write event
POST /event/create       | Create events
GET /event/events        | List last 100 events
GET /event/stats         | Count events per type
GET /user/users/{userId} | Return user balance
PUT /user/updateBalance  | Update user balance

1.2 Data Engineering Tasks (Python 3.13)

Python scripts process mock datasets (CSV files) and perform analytical tasks:

- Duplicate & False-Success Cleaning  
  Deduplicate purchases_raw and remove false success states.

- Reconciliation (±10 min)  
  Join purchases_raw with confirmed_purchases.csv and classify entries:

    • matched  
    • af_only  
    • confirmed_only

- ROAS + Anomaly Detection  
  Compute D-1 ROAS and flag values below 50% of the 7-day average.

- ARPDAU Calculation (D-1)  
  Compute:

    • Daily Active Users (DAU)  
    • Average Revenue Per Daily Active User (ARPDAU)


------------------------------------------------------------
2. Project Structure
------------------------------------------------------------

The following is the full hierarchical directory structure:

```bash
GameBackend/
├─ GameBackend.API/
│  ├─ Controllers/
│  ├─ Data/
│  ├─ DTO/
│  ├─ Helpers/
│  ├─ Migrations/
│  ├─ Models/
│  ├─ Services/
│  ├─ appsettings.json
│  ├─ DockerFile
│  └─ Program.cs
│
├─ GameData/
│  ├─ Files/
│  │  ├─ confirmed_purchases.csv
│  │  ├─ costs_daily.csv
│  │  ├─ purchases.csv
│  │  └─ sessions.csv
│  │
│  ├─ DockerFile
│  ├─ GameData.py
│  └─ requirements.txt
│
├─ docker-compose.yml
└─ README.md
```


------------------------------------------------------------
3. Running the Project
------------------------------------------------------------

3.1 Start all services

docker compose up --build


3.2 Services

Service                | URL
-----------------------|---------------------------------------
Backend API (.NET 10)  | http://localhost:7239
Python Worker          | Runs automatically via entrypoint
Database               | SQL Server (runs inside API image)


------------------------------------------------------------
4. Data Model
------------------------------------------------------------

4.1 User Model

```bash
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public int Balance { get; set; } = 0;
}
```

4.2 Event Model

```bash
public class Event
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string EventType { get; set; } = null!;
    public DateTime TsUtc { get; set; }
    public string? Meta { get; set; }
}
```

------------------------------------------------------------
5. Data Transfer Objects (DTOs)
------------------------------------------------------------

```bash
public record RegisterDto(string Username, string Password);
public record LoginDto(string Username, string Password);
public record UserDto(int UserId, int Balance);
public record EventDto(int UserId, string EventType, string Meta, DateTime TsUtc);
public record EarnDto(int UserId, int Amount, string Reason);
```

------------------------------------------------------------
6. Outputs Produced by Python Scripts
------------------------------------------------------------

All outputs are stored under:
/GameBackend/GameData/Reports/

Task                | Output
--------------------|--------------------------
Deduplication       | purchases_curated.csv
Reconciliation      | reconciliation.json
ROAS                | roas_d1.json
Anomaly Detection   | roas_anomaly.json
ARPDAU              | arpdau_d1.json
