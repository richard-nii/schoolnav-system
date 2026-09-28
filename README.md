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

- Visual Studio 2022 (17.8+) with the **ASP.NET and web development** workload, OR the
  .NET 8 SDK + VS Code
- SQL Server LocalDB (installed automatically with Visual Studio) — or point the
  connection string at any SQL Server instance you have

## First-time setup

1. Open `SchoolNavigationSystem.sln` in Visual Studio.
2. Let NuGet restore packages (this needs internet access — it will not work in a
   sandboxed environment with no NuGet access).
3. No manual migration step needed for a first run — the API calls
   `Database.EnsureCreatedAsync()` on startup and seeds sample University of Ghana
   data automatically (see `Data/SeedData.cs`). If you'd rather use proper EF
   migrations (recommended if this grows past a class project), run:
   ```
   dotnet ef migrations add InitialCreate --project src/SchoolNav.Api
   dotnet ef database update --project src/SchoolNav.Api
   ```
   and remove the `EnsureCreatedAsync()` call in `Program.cs`.
4. In Visual Studio, right-click the solution → **Set Startup Projects** → **Multiple
   startup projects** → set both `SchoolNav.Api` and `SchoolNav.Client` to **Start**.
5. Press **F5**. Two browser tabs should open: the API's Swagger page
   (`https://localhost:7200/swagger`) and the Blazor app (`https://localhost:7100`).

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
