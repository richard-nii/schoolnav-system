# School Navigation System — University of Ghana, Legon

A web app for finding campus locations and getting walking directions, built with
ASP.NET Core Web API + Blazor WebAssembly + SQL Server.

## Solution structure

```
SchoolNavigationSystem.sln
src/
  SchoolNav.Api/       ASP.NET Core Web API (EF Core, Identity + JWT, Dijkstra pathfinding)
  SchoolNav.Client/    Blazor WebAssembly frontend (Leaflet map)
```

## Prerequisites

- .NET 10 SDK + VS Code (or Visual Studio 2022 on Windows) with the C# Dev Kit
- Docker Desktop, for running PostgreSQL locally (the project uses PostgreSQL, not
  SQL Server — see "Why PostgreSQL" below)

## First-time setup

1. Start a local PostgreSQL container:
   ```
   docker run -e POSTGRES_PASSWORD=YourStrong!Passw0rd -e POSTGRES_DB=SchoolNavDb -p 5432:5432 --name schoolnav-pg -d postgres:16
   ```
   (Postgres's official image runs natively on Apple Silicon, no `--platform` flag needed.)
2. Open the `SchoolNavigationSystem` folder in VS Code, or the `.sln` in Visual Studio.
3. Run `dotnet restore` from the repo root.
4. No manual migration step needed for a first run — the API calls
   `Database.EnsureCreatedAsync()` on startup and seeds sample University of Ghana
   data automatically (see `Data/SeedData.cs`). If you'd rather use proper EF
   migrations (recommended if this grows past a class project), run:
   ```
   dotnet ef migrations add InitialCreate --project src/SchoolNav.Api
   dotnet ef database update --project src/SchoolNav.Api
   ```
   and remove the `EnsureCreatedAsync()` call in `Program.cs`.
5. Run the API: `dotnet run --project src/SchoolNav.Api` (listens on `https://localhost:7200`).
6. In a second terminal, run the client: `dotnet run --project src/SchoolNav.Client`
   (listens on `https://localhost:7100`).

## Why PostgreSQL instead of SQL Server?

The project was originally built against SQL Server per the brief. It was switched to
PostgreSQL specifically to deploy for free on Render.com without a credit card (Azure's
free SQL Server-compatible tier requires student/account verification). If you get
Azure access later, switching back is a small change: swap the `Npgsql.EntityFrameworkCore.PostgreSQL`
package for `Microsoft.EntityFrameworkCore.SqlServer`, change `UseNpgsql` to `UseSqlServer`
in `Program.cs`, and update the connection string format.

## Default admin login

```
Email:    admin@ug.edu.gh
Password: Admin@12345
```
**Change this password** (or at minimum the JWT signing key in `appsettings.json`)
before sharing this anywhere beyond your own machine.

## What's seeded out of the box

10 real UG Legon landmarks (Balme Library, Great Hall, Commonwealth Hall, UGBS,
UGCS, Volta Hall, Akuafo Hall, the Computer Science department, the Night Market,
and the Main Gate), connected by a small walkway graph of 8 nodes/9 edges.

**Important:** the seeded coordinates are realistic approximations, not
survey-accurate GPS pins. Use the Admin Dashboard → Manage Locations /
Manage Walkway Graph pages (click-on-map coordinate picker) to correct or expand
them to match the real campus as precisely as you'd like.

## How routing works

1. Every `Location` (a classroom, office, etc.) is linked to its `NearestNode` —
   the closest point on the walkway graph.
2. `POST /api/route?fromId=&toId=` runs Dijkstra's algorithm
   (`Services/PathfindingService.cs`) over the `Node`/`Edge` graph to find the
   shortest walking path between the two locations' nearest nodes.
3. The API returns an ordered list of GPS points; the Blazor client draws them as a
   polyline on the Leaflet map.

To make routes more realistic, add more Nodes along actual walkways (paths,
sidewalks, road crossings) in the admin dashboard — the more nodes/edges you add,
the more the route will hug real walkable paths instead of cutting through
buildings.

## Extending this for your report/demo

- Add more locations/buildings via the admin dashboard, no code changes needed.
- The `LocationCategory` enum (`Models/Location.cs`) controls the dropdown/filter
  categories — add more if you need finer-grained categories.
- `PathfindingService.HaversineMeters` is also handy if you want to show
  "distance as the crow flies" vs. "walking distance" as a comparison in your demo.
