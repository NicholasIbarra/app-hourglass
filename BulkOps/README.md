# Bulk Ops (Aspire + EF Core + Hangfire)

This sample is a proof-of-concept for high-volume imports using EF Core bulk operations.

## What this sample includes

- .NET Aspire orchestration (`BulkOps.AppHost`)
- SQL Server container wired through Aspire resource references
- Minimal API (`BulkOps.Api`)
- EF Core DbContext with `User`, `Office`, and `UserOffice` (many-to-many via assignment table)
- Repository pattern for bulk inserts via `EFCore.BulkExtensions`
- Fake data generation via `Bogus` (defaults to 5000 users)
- Background import pipeline using Hangfire + SQL Server storage

## Projects

- `src/BulkOps.AppHost` - Starts SQL Server container + API project.
- `src/BulkOps.Api` - API endpoints, EF model, repositories, fake generators, Hangfire jobs.
- `src/BulkOps.ServiceDefaults` - Shared Aspire service defaults.

## Triggering imports

Queue a background import job:

```http
POST /imports/users?count=5000
```

Open Hangfire dashboard:

```http
GET /jobs
```

## Notes

- This is scaffold-level code intended as a recipe project inside `app-hourglass`.
- Migrations are not yet added; the API currently uses `Database.EnsureCreated()`.
