# dotnet-courses-web

ASP.NET Core MVC web application for managing courses, groups, students and teachers built with Entity Framework Core and Bootstrap.

## Features

- **Groups**: accordion list with inline edit (name, teacher), delete protection when students exist, clear all students action, filter by course and student count
- **Students**: inline CRUD, search by name with debounce, filter by group, pagination
- **Teachers**: inline CRUD, search by name with debounce, pagination
- AJAX-based updates — no full page reloads
- Bootstrap 5 modals for delete/clear confirmations
- Server-side pagination with numbered buttons and ellipsis
- SQL Server database with EF Core migrations and seed data
- Serilog structured logging

## Stack

- .NET 10 / C#
- ASP.NET Core MVC
- Entity Framework Core + SQL Server
- Serilog
- Bootstrap 5

## Project Structure

```
src/
├── CoursesApp.Domain/         # Entities, interfaces, enums
├── CoursesApp.Infrastructure/ # EF Core, repositories, migrations
└── CoursesApp.Web/            # Controllers, views, services, DTOs
```

## Database

Start SQL Server via Docker:

```bash
docker-compose up -d
```

## Configuration

Fill in connection string in `src/CoursesApp.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoursesDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "Pagination": {
    "PageSize": 5
  }
}
```

## Migrations

```bash
cd src
dotnet ef migrations add InitialCreate --project CoursesApp.Infrastructure --startup-project CoursesApp.Web
dotnet ef database update --project CoursesApp.Infrastructure --startup-project CoursesApp.Web
```

## Run

```bash
dotnet run --project src/CoursesApp.Web
```