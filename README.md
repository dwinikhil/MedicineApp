ABC Pharmacy - Single Page Application + .NET Minimal Web API
============================================================

This project implements the requirements from the provided brief. See original brief: fileciteturn0file0

Contents
--------
- Program.cs         : Minimal API + simple JSON-backed store.
- MedicineApp.csproj : .NET project file (target .NET 7.0)
- Data/medicines.json : JSON data store (sample data).
- wwwroot/index.html, app.js, styles.css : Single-page frontend (vanilla JavaScript).
- README.md          : This file.

Prerequisites
-------------
- .NET 7 SDK installed. (If you have .NET 6 only, change TargetFramework in the .csproj.)
- A terminal / command prompt.

How to run
----------
1. Unzip the project folder.
2. From project root (where the .csproj is), run:

   dotnet restore
   dotnet run

3. By default the app will print a URL such as https://localhost:5001 or http://localhost:5000.
   Open that URL in your browser. The SPA is served from the same host and calls the API endpoints under /api.

Endpoints
---------
- GET  /api/medicines            => list all medicines. Supports optional query string 'q' for name search.
- GET  /api/medicines/{id}     => get single medicine
- POST /api/medicines            => add a medicine (JSON body)
- POST /api/sales                => record a sale { medicineId: guid, quantity: number } — reduces quantity and persists.

Data storage
------------
Data is stored as JSON in the Data/medicines.json file and Data/sales.json for sales. The API reads/writes these files.

Notes & assumptions
ABC Pharmacy - Single Page Application + .NET Minimal Web API
============================================================

This project is a small SPA (vanilla JavaScript) coupled with a .NET minimal Web API that stores data in JSON files for simplicity.

Summary of changes and features
-------------------------------
- Project now targets `.NET 8.0`.
- Client and server-side validation for all medicine fields (FullName, Notes, ExpiryDate, Quantity, Price, Brand).
- Frontend grid with color coding:
   - Red: expiry date less than 30 days from today (takes precedence).
   - Yellow: quantity in stock less than 10.
- The app seeds a `Test Medicine` if the data file is missing or corrupted so the UI works immediately.
- `/health` endpoint added and JSON serialization configured to camelCase to match the frontend.

Contents
--------
- `Program.cs`         : Minimal API + JSON-backed store and validation.
- `MedicineApp.csproj` : .NET project file (target `net8.0`).
- `Data/medicines.json`: JSON data store.
- `Data/sales.json`    : Sales log appended on sales.
- `wwwroot/index.html`, `wwwroot/app.js`, `wwwroot/styles.css` : SPA frontend.

Prerequisites
-------------
- .NET SDK 8.0 (or higher). Check installed runtimes:

```powershell
dotnet --list-runtimes
```

How to run
----------
1. From project root (where `MedicineApp.csproj` is), restore and build:

```powershell
dotnet restore
dotnet build
```

2. Run the app (keep this terminal open to see logs):

```powershell
dotnet run --project "c:\Users\HP\Downloads\MedicineApp\MedicineApp.csproj"
```

3. The app prints the listening URL (for example `http://localhost:5000`). Open that in your browser.

Notes
-----
- Keep the `dotnet run` terminal dedicated to the running app. Run HTTP tests in a separate terminal to avoid accidentally interrupting the server (Ctrl+C).

API endpoints
-------------
- `GET  /api/medicines`            — list all medicines. Supports optional `?q=` for name search.
- `GET  /api/medicines/{id}`       — get a single medicine.
- `POST /api/medicines`            — add a medicine (JSON body). Server validates fields.
- `POST /api/sales`                — record a sale `{ medicineId: guid, quantity: number }` — reduces quantity and persists to `Data/sales.json`.
- `GET  /health`                   — health check: returns `{ status: "ok" }`.

Example requests (PowerShell)
----------------------------
- List medicines:

```powershell
Invoke-RestMethod -Uri 'http://localhost:5000/api/medicines' -UseBasicParsing | ConvertTo-Json -Depth 5
```

- Add a medicine:

```powershell
$body = @{ fullName = 'Unit Test Med'; notes = 'notes'; expiryDate = '2026-12-01'; quantity = 10; price = 5.25; brand = 'UnitBrand' } | ConvertTo-Json
Invoke-RestMethod -Uri 'http://localhost:5000/api/medicines' -Method Post -Body $body -ContentType 'application/json'
```

- Record a sale:

```powershell
$sale = @{ medicineId = '<guid-from-list>'; quantity = 1 } | ConvertTo-Json
Invoke-RestMethod -Uri 'http://localhost:5000/api/sales' -Method Post -Body $sale -ContentType 'application/json'
```

Frontend behavior
-----------------
- Grid columns: Name, Expiry, Qty, Price (2 decimals), Brand, Action.
- Color rules: Red = expiry < 30 days; Yellow = quantity < 10. Red takes precedence.
- Search box calls `GET /api/medicines?q=term` as you type.
- Add-medicine modal performs client-side validation for all fields and shows inline errors.

Data persistence
----------------
- `Data/medicines.json` stores medicines (cleaned/seeded on startup if missing/corrupt).
- `Data/sales.json` appends sales records.

- Troubleshooting
- ---------------
- This project requires **.NET 8.0**.
- If you do not have .NET 8 installed, download and install the .NET 8 SDK from https://dotnet.microsoft.com/download and then rebuild.
- Do not downgrade the project to an earlier runtime — the code and project are built and tested against .NET 8.
- If `dotnet build` complains the DLL is locked, stop any running instance of the app before building:

```powershell
Get-Process -Name dotnet -ErrorAction SilentlyContinue | Where-Object { $_.Path -like '*MedicineApp*' } | Stop-Process -Force
```

- To see detailed exceptions, run the app in Development mode:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project 'c:\Users\HP\Downloads\MedicineApp\MedicineApp.csproj'
```

Security & production notes
--------------------------
- This demo uses JSON files for storage. Use a database for production workloads.
- No authentication or access control is implemented. Add appropriate security before exposing publicly.