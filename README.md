ABC Pharmacy - Single Page Application + .NET Minimal Web API
============================================================

This project is a small SPA (vanilla JavaScript) coupled with a .NET minimal Web API that stores data in JSON files for simplicity.

Summary of changes and features
-------------------------------
- Project now targets `.NET 8.0`.
- Client and server-side validation for all medicine fields (FullName, Notes, ExpiryDate, Quantity, Price, Brand).
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
# ABC Pharmacy — MedicineApp

A small Single-Page Application (Vanilla JavaScript) served by a .NET minimal Web API. The app stores data as JSON files in the `Data/` folder for simplicity — this repository is a demo and intended for local development and learning.

**What this repo contains:**
- `Program.cs` — minimal Web API and server logic
- `MedicineApp.csproj` — project file (targets .NET 8.0)
- `wwwroot/` — frontend static files (`index.html`, `app.js`, `styles.css`)
- `Data/` — JSON data files used by the API (`medicines.json`, `sales.json`)

**Audience:** developers who want to run, inspect, or extend a simple SPA + API demo.

**High-level features:**
- Searchable medicine list, add medicine, record sales
- Simple JSON-backed persistence (no database)
- Frontend written in plain (vanilla) JavaScript

**Important note:** the project targets `.NET 8.0`. Use .NET 8 SDK to build and run.

**Supported platforms:** Windows, macOS, Linux (where .NET 8 SDK is available).

**Quick table of contents**
- **Prerequisites**
- **Quick start (run)**
- **Development commands**
- **API endpoints**
- **Data files**
- **Troubleshooting & tips**

**Prerequisites**
- .NET SDK 8.0 or later. Verify installed SDKs:

```
dotnet --list-sdks
```

- No Node.js / npm is required to run the app. The frontend is static files served from `wwwroot/`.

**Quick start — run locally**
1. Open a terminal in the project root (where `MedicineApp.csproj` is located).
2. Restore, build and run:

```powershell
dotnet restore
dotnet build
dotnet run
```

3. The server will print a local URL (for example `http://localhost:5000` or `https://localhost:5001`). Open that URL in your browser.

**Useful development commands**
- Restore packages: `dotnet restore`
- Build: `dotnet build`
- Run: `dotnet run`
- List NuGet packages: `dotnet list package`
- Add a package: `dotnet add package <PackageName>`
- Check for outdated packages: `dotnet list package --outdated`

**API endpoints**
All endpoints are prefixed with `/api` unless noted.

- `GET  /api/medicines` — list all medicines. Optional query `?q=` filters by name.
- `GET  /api/medicines/{id}` — get a single medicine by id.
- `POST /api/medicines` — add a medicine (JSON body). Validates required fields.
- `POST /api/sales` — record a sale `{ medicineId: string, quantity: number }`. Reduces quantity and appends a record to `Data/sales.json`.
- `GET  /health` — basic health check (returns JSON `{ status: "ok" }`).

**Data files**
- `Data/medicines.json` — the medicines store. The API reads and writes this file.
- `Data/sales.json` — appended sales log (created when the first sale is recorded).

The files are plain JSON. For production use, replace this with a proper database.

**Frontend**
- The SPA lives in `wwwroot/`. Open [wwwroot/index.html](wwwroot/index.html) in a browser when the server is running.