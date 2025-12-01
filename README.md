Backend & Data Engineering Assignment
(.NET 10 + Python 3.13 • Docker Compose)

This repository implements the Backend & Data Engineering technical assignment using .NET 10 for the core game backend API and Python 3.13 for the data-processing tasks. The entire system runs using Docker Compose, following the requirements defined in the project brief.

Project Overview

The system consists of two main components:

1. Core Game Backend API (.NET 10)

Simulates a lightweight game backend similar to PlayFab/Azure environments.

Supports:

Endpoint	Description
POST /login	Register a user & return JWT
POST /earn	Increase user balance + write event
POST /event	Store arbitrary events
GET /balance	Return user balance
GET /events	List last 100 events
GET /stats	Count events per type
GET /track?character=<ID>	Capture deep-link parameter

All timestamps are stored in UTC.

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

✓ Deep-Link Parameter Flow

The backend API exposes /track and Python scripts validate the flow for character=<ID>.

Project Architecture

root/
 ├─ backend/              # .NET 10 Web API
 │   ├─ Controllers/
 │   ├─ Models/
 │   ├─ Services/
 │   ├─ Data/
 │   └─ ...               
 │
 ├─ data_tasks/           # Python 3.13 data engineering scripts
 │   ├─ cleaning/
 │   ├─ reconciliation/
 │   ├─ reporting/
 │   ├─ anomaly_detection/
 │   └─ ...
 │
 ├─ sample_data/          # Provided CSV files
 │   ├─ confirmed_purchases.csv
 │   ├─ costs_daily.csv
 │   └─ sessions.csv
 │
 ├─ docker-compose.yml
 └─ README.md


Run the Project

1. Start all services

docker compose up --build

2. Services

| Service                | URL                               |
| ---------------------- | --------------------------------- |
| Backend API (.NET 10)  | `http://localhost:5000`           |
| Python Worker          | Runs automatically via entrypoint |
| Database (if included) | ⚠️ EDIT ME                        |


Authentication (JWT)

POST /login returns a JWT token.
Use it in subsequent requests:

Authorization: Bearer <token>

The backend uses a lightweight JWT issuer for simulation purposes.

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


Outputs Produced by Python Scripts

| Task              | Output                     |
| ----------------- | -------------------------- |
| Deduplication     | `purchases_curated.json`   |
| Reconciliation    | `reconciliation.json`      |
| ROAS              | `roas_d1.json`             |
| Anomaly Detection | `roas_anomaly.json`        |
| ARPDAU            | `arpdau_d1.json`           |


All outputs are stored under:
/GameBackend/GameData/Reports/

