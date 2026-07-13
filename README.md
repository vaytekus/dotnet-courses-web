# dotnet-courses-web

ASP.NET Core MVC web application for managing courses, groups, students and teachers.

## Features

- Browse courses with expandable groups
- Add, edit, delete students, teachers and groups (inline, without page reload)
- Filter and search with server-side pagination
- Import / export students per group as CSV
- ASP.NET Core Identity: register, login, logout with email confirmation (SMTP)
- Rate limiting, server-side caching, CancellationToken through all layers
- Unit tests (xUnit + Moq)

## Stack

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core + SQL Server
- ASP.NET Core Identity
- MailKit (SMTP / Gmail)
- Serilog / Bootstrap 5

## Project Structure

```
src/
├── CoursesApp.Domain/         # Entities, interfaces
├── CoursesApp.Infrastructure/ # EF Core, repositories, migrations
└── CoursesApp.Web/            # Controllers, views, services
tests/
└── CoursesApp.Tests/          # Unit tests for services
```

## Database

Start SQL Server via Docker:

```bash
docker-compose up -d
```

## Configuration

Copy and fill in `appsettings.Development.example.json` → `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoursesDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "FromEmail": "your@gmail.com",
    "UserName": "your@gmail.com",
    "Password": "YOUR_GMAIL_APP_PASSWORD"
  }
}
```

## Run

```bash
dotnet run --project src/CoursesApp.Web
dotnet test tests/CoursesApp.Tests
```