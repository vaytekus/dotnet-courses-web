# dotnet-courses-web

ASP.NET Core MVC web application for managing courses, groups, students and teachers built with Entity Framework Core and Bootstrap.

## Features

- Browse courses with expandable groups via accordion
- View students per group loaded dynamically via AJAX
- Inline editing of students and teachers (edit/save/cancel without page reload)
- Delete students and teachers with confirmation dialog
- Filter students by group
- SQL Server database with EF Core migrations and seed data

## Stack

- .NET 10 / C#
- ASP.NET Core MVC
- Entity Framework Core + SQL Server
- Serilog
- Bootstrap 5

## Project Structure

```
src/
├── CoursesApp.Domain/         # Entities, interfaces
├── CoursesApp.Infrastructure/ # EF Core, repositories, migrations
└── CoursesApp.Web/            # Controllers, views, services
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
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
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