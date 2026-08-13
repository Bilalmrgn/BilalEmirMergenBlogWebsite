# Bilal Emir Mergen — Portfolio

A production-oriented, database-driven personal portfolio built with ASP.NET Core 9, Entity Framework Core, and Microsoft SQL Server.

## Architecture

- **Public site:** server-rendered MVC pages for fast first render, accessible navigation, and reliable SEO.
- **Admin:** authenticated MVC workspace at `/admin/login` with CRUD, publishing, image uploads, previews, and drag-to-reorder lists.
- **REST API:** read-only public endpoints under `/api` and JWT-protected management endpoints under `/api/admin`.
- **Data:** EF Core with SQL Server migrations. Public components never contain mock content; all content comes from the database.
- **Services:** password hashing, JWT creation, validated image/PDF storage, seed data, and API exception handling are isolated from controllers.

## Required configuration

Use environment variables or .NET user secrets. Do not commit production secrets.

```text
ConnectionStrings__DefaultConnection=Server=...;Database=...;...
InitialAdmin__Username=admin
InitialAdmin__Email=admin@example.com
InitialAdmin__Password=use-a-long-unique-password
Jwt__Key=use-a-random-secret-at-least-32-characters-long
Jwt__Issuer=BilalEmirMergenWebsite
Jwt__Audience=BilalEmirMergenWebsite.Admin
```

The initial administrator is created or updated during startup only when the three `InitialAdmin` values are present. Production startup requires `Jwt__Key`.

## Run locally

```powershell
dotnet restore .\BilalEmirMergenWebsite.sln
dotnet ef database update --project .\BilalEmirMergenWebsite\BilalEmirMergenWebsite.csproj
dotnet run --project .\BilalEmirMergenWebsite\BilalEmirMergenWebsite.csproj
```

The default local URL is `http://localhost:5122`. Health checks are available at `/health`.

## API overview

Public endpoints include `/api/about`, `/api/experiences`, `/api/educations`, `/api/skill-categories`, `/api/projects`, `/api/languages`, `/api/blog`, and `/api/social-links`.

Request a two-hour admin access token with `POST /api/admin/auth/login`, then send it as `Authorization: Bearer <token>` to the protected management endpoints.

## Production hosting

Deploy the release output to an ASP.NET Core 9 host with a reachable SQL Server database. Keep `wwwroot/uploads` on persistent storage (or replace the local image service with object storage) so admin uploads survive new releases. Terminate TLS at the host or reverse proxy and provide all required configuration through the host's secret manager.
