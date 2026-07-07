# dotnet-courses-web

ASP.NET Core MVC web application for managing courses, groups, students and teachers.

## Features

- Browse courses with expandable groups via accordion
- View, add, edit, delete students and teachers (inline, without page reload)
- Filter and search groups, students, teachers with server-side pagination
- Import / export students per group as CSV
- ASP.NET Core Identity authentication (register, login, logout)
- Email confirmation on registration via SMTP (Gmail)
- Rate limiting (100 req/min per route)
- Server-side caching with IMemoryCache
- CancellationToken propagation through all layers

## Stack

- .NET 10 / C#
- ASP.NET Core MVC
- Entity Framework Core + SQL Server
- ASP.NET Core Identity
- MailKit (SMTP)
- Serilog
- Bootstrap 5
- xUnit + Moq (unit tests)

## Project Structure

```
src/
├── CoursesApp.Domain/         # Entities, repository interfaces
├── CoursesApp.Infrastructure/ # EF Core, repositories, migrations, seeder
└── CoursesApp.Web/            # Controllers, views, services, DTOs, mappers
tests/
└── CoursesApp.Tests/          # Unit tests for services
```

## Database

Start SQL Server via Docker:

```bash
docker-compose up -d
```

## Configuration

Create `src/CoursesApp.Web/appsettings.Development.json` and fill in your values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoursesDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "FromEmail": "your@gmail.com",
    "FromName": "CoursesApp",
    "UserName": "your@gmail.com",
    "Password": "YOUR_GMAIL_APP_PASSWORD"
  }
}
```

> For Gmail, generate an **App Password** at Google Account → Security → 2-Step Verification → App passwords.

## Apply migrations

```bash
cd src
dotnet ef database update --project CoursesApp.Infrastructure --startup-project CoursesApp.Web
```

## Run

```bash
dotnet run --project src/CoursesApp.Web
```

## Tests

```bash
dotnet test tests/CoursesApp.Tests
```