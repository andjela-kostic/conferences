# Conference Management API

A REST API for managing conferences and their topics, built with ASP.NET Core 10 and PostgreSQL. It uses a modular architecture with `Domain`, `Application`, `Infrastructure`, and `API` projects.

## 🚀 Features

- ✅ Full CRUD operations for conferences, topics, and users
- ✅ Many-to-many relationship between users and topics
- ✅ Topic registration and unregistration for conference attendees
- ✅ PostgreSQL database with Entity Framework Core
- ✅ Swagger/OpenAPI documentation
- ✅ Modular architecture with DTOs, services, repositories, and controllers

## 🛠️ Tech Stack

- **.NET 10 (LTS)** — SDK version pinned in `global.json`
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **PostgreSQL** (via Npgsql EF Core provider)
- **Swagger/OpenAPI**

Package versions are managed centrally in `Directory.Packages.props`; shared build settings
(target framework, nullable, warnings as errors, code style) live in `Directory.Build.props`.

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (see `global.json` for the exact version)
- [PostgreSQL](https://www.postgresql.org/download/)
- IDE: Visual Studio, Rider, or VS Code

## ⚙️ Setup & Installation

### 1. Clone the repository
```bash
git clone https://github.com/andjela-kostic/conferences.git
cd conferences
```

### 2. Configure Database Connection

The non-secret part of the connection string (host, port, database, user) is committed in
`API/appsettings.Development.json`. The password is **never** stored in the repository.

Store the full connection string locally with [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ConferencesDb;Username=postgres;Password=YOUR_PASSWORD" --project API
```

User secrets are stored outside the project folder and override `appsettings*.json` in the Development environment.

In other environments, provide the connection string through an environment variable:

```bash
export ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=ConferencesDb;Username=...;Password=..."
```

### 3. Apply Database Migrations

`dotnet-ef` is a local tool pinned in `dotnet-tools.json`:

```bash
dotnet tool restore
dotnet ef database update --project Infrastructure --startup-project API
```

### 4. Run the Application
```bash
dotnet run --project API/API.csproj
```

The API will be available at:
- HTTP: `http://localhost:5018`
- HTTPS: `https://localhost:7241`
- Swagger UI: `http://localhost:5018/swagger/index.html`

## 📚 API Endpoints

### Conferences
- `GET /api/conferences` - Get all conferences
- `POST /api/conferences` - Create a new conference
- `PUT /api/conferences/{id}` - Update a conference
- `DELETE /api/conferences/{id}` - Delete a conference

### Topics
- `GET /api/topics` - Get all topics
- `GET /api/topics/conference/{conferenceId}` - Get topics for a specific conference
- `POST /api/topics` - Create a new topic
- `PUT /api/topics/{id}` - Update a topic
- `DELETE /api/topics/{id}` - Delete a topic

### Users
- `GET /api/users` - Get all users
- `POST /api/users` - Register a new user
- `POST /api/users/register-topic` - Register user to a topic
- `POST /api/users/unregister-topic` - Unregister user from a topic
- `GET /api/users/{userId}/topics` - Get all topics a user is registered for
- `GET /api/users/topic/{topicId}` - Get all users registered for a topic

## 🧪 Testing

### Automated tests

Unit tests for the application services live in `tests/Application.UnitTests` (xUnit v3, in-memory fake repositories, no database needed):

```bash
dotnet test --solution Conferences.sln
```

### Manual requests

Use the included `API/Conferences.http` file with REST Client extension or similar tools to test all endpoints.
