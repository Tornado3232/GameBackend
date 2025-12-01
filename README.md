Backend & Data Engineering Assignment
(.NET 10 + Python 3.13 • Docker Compose)

This repository implements the Backend & Data Engineering technical assignment using .NET 10 for the game backend API and Python 3.13 for the data reliability tasks. 
The entire system runs using Docker Compose, following the requirements defined in the project brief.

Project Overview

1. Core Game Backend API (.NET 10)

| Endpoint               | Description                          |
| ---------------------- | ------------------------------------ |
| POST /register         | Register a user                      |
| POST /login	         | User login & return JWT              |
| POST /earn	         | Increase user balance + write event  |
| POST /event	         | Store arbitrary events               |
| GET /balance	         | Return user balance                  |
| GET /events	         | List last 100 events                 |
| GET /stats	         | Count events per type                |
	
2. Data Engineering Tasks (Python 3.13)

Python scripts process mock datasets (CSV files) and perform analytical tasks:

✓ Duplicate & False-Success Cleaning

Deduplicate purchases_raw, remove false success states.

✓ Reconciliation (±10 min)

Join purchases_raw with confirmed_purchases.csv and classify:

matched

af_only

confirmed_only

✓ ROAS + Anomaly Detection

Compute D-1 ROAS, flag values below 50% of the 7-day average.

✓ ARPDAU Calculation (D-1)

Compute:

Daily Active Users (DAU)

Average Revenue Per Daily Active User (ARPDAU)

Project Architecture

GameBackend/
 ├─ GameBackend.API/
 │   ├─ Controllers/
 │   ├─ Data/
 │   ├─ DTO/
 │   ├─ Helpers/
 │   ├─ Migrations/
 │   ├─ Models/
 │   ├─ Services/
 │   ├─ appsettings.json
 │   ├─ DockerFile
 │   └─ Program.cs
 │
 ├─ GameData/
 │   ├─ Files/
 │   |   ├─ confirmed_purchases.csv
 │   │   ├─ costs_daily.csv
 │   │   ├─ purchases.csv
 │   │   └─ sessions.csv
 │   │
 |   ├─ DockerFile
 │   ├─ GameData.py
 │   └─ requirements.txt
 │
 ├─ docker-compose.yml
 └─ README.md


Run the Project

1. Start all services

docker compose up --build

2. Services

| Service                | URL                               |
| ---------------------- | --------------------------------- |
| Backend API (.NET 10)  | `http://localhost:7239`           |
| Python Worker          | Runs automatically via entrypoint |
| Database (if included) | Sql Server(Runs on Backend Server)|


Data Model

User

public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int Balance { get; set; } = 0;
    }

Event

public class Event
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EventType { get; set; } = null!;
        public DateTime TsUtc { get; set; }
        public string? Meta { get; set; }
    }

Data Transfer Objects (DTOs)

    public record RegisterDto(string Username, string Password);
    public record LoginDto(string Username, string Password);
    public record UserDto(int UserId, int Balance);
    public record EventDto(int UserId, string EventType, string Meta, DateTime TsUtc);
    public record EarnDto(int UserId, int Amount, string Reason);
    
Outputs Produced by Python Scripts

All outputs are stored under:
/GameBackend/GameData/Reports/

| Task              | Output                     |
| ----------------- | -------------------------- |
| Deduplication     | `purchases_curated.json`   |
| Reconciliation    | `reconciliation.json`      |
| ROAS              | `roas_d1.json`             |
| Anomaly Detection | `roas_anomaly.json`        |
| ARPDAU            | `arpdau_d1.json`           |

