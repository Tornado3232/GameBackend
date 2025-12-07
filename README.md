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
GET /user/{userId}       | Return user balance
PUT /user/update         | Update user balance

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
│
├─ GameBackend.API/
│  ├─ Abstractions/
│  ├─ Controllers/
│  ├─ Data/
│  ├─ DTO/
│  ├─ Helpers/
│  ├─ Migrations/
│  ├─ Models/
│  ├─ Services/
│  ├─ .env
│  ├─ appsettings.json
│  ├─ DockerFile
│  └─ Program.cs
│
├─ GameBackend.API.Test/
│  ├─ Services/
│  ├─ AuthenticationTest.cs
│  └─ EarnTest.cs
│
├─ GameData/
│  ├─ Files/
│  │  ├─ confirmed_purchases.csv
│  │  ├─ costs_daily.csv
│  │  ├─ purchases.csv
│  │  └─ sessions.csv
│  │
│  ├─ Reports/
│  │  ├─ arpdau_d1.json
│  │  ├─ purchases_curated.csv
│  │  ├─ reconciliation.json
│  │  ├─ roas_anomaly.json
│  │  └─ roas_d1.json
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

```bash
docker compose up
```

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


4.3 IdempotencyRecord Model

```bash
public class IdempotencyRecord
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string ResponseBody { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
```

4.4 AppsFlyer Model

```bash
public class AppsFlyer
{
    public int Id { get; set; }
    public string Payload { get; set; }
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

------------------------------------------------------------
7. API Full Flow
------------------------------------------------------------

                                                      7.1 User Register


<img width="857" height="515" alt="Auth_Register" src="https://github.com/user-attachments/assets/7a859f3c-0c6e-4372-9eb7-09ceab6dfcef" />


                                                        
                                                        7.2 User Login

                                                        
<img width="857" height="496" alt="Auth_Login" src="https://github.com/user-attachments/assets/d934ea20-1717-4e29-a85d-18aff0e15c60" />



                                                      7.3 Create an Event

                                                      
<img width="858" height="566" alt="Event_Create" src="https://github.com/user-attachments/assets/0a3dab9b-6591-4267-bf1e-60e41c2d0f59" />



                                                        7.4 Get Events

                                                        
<img width="928" height="609" alt="Event_Get" src="https://github.com/user-attachments/assets/cb513360-342f-4f43-a826-f227e02495ec" />



                                                        7.5 Get Stats

                                                        
<img width="937" height="419" alt="Event_Get_Stats" src="https://github.com/user-attachments/assets/018c9027-2a9f-44c6-a7c9-4e40e329116a" />



                                                          7.6 Earn

                                                          
<img width="941" height="504" alt="Earn_Earn" src="https://github.com/user-attachments/assets/d2d7a555-38e3-49e3-a8db-211ad7ced4fc" />



                                                    7.7 Get User's Balance

                                                    
<img width="942" height="325" alt="User_Get_Balance" src="https://github.com/user-attachments/assets/f6cf026c-f804-48b1-a179-9b1f1a36e025" />



                                                    7.8 Update User's Balance

                                                    
<img width="941" height="464" alt="User_Update_Balance" src="https://github.com/user-attachments/assets/ec98bfe8-5f47-41bd-acc1-5a6169729f0d" />



                                                      7.9 AppsFlyer Postback


<img width="938" height="592" alt="AppsFlyer_Postback" src="https://github.com/user-attachments/assets/ae4833aa-6f0e-47a2-be60-817c4dd58a98" />





