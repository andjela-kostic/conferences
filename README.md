# Conference Management API

A REST API for managing conferences and their topics, built with ASP.NET Core 9 and PostgreSQL. Allows users to register for specific conference topics with many-to-many relationships.

## 🚀 Features

- ✅ Full CRUD operations for conferences, topics, and users
- ✅ Many-to-many relationship between users and topics
- ✅ Topic registration and unregistration for conference attendees
- ✅ PostgreSQL database with Entity Framework Core
- ✅ Swagger/OpenAPI documentation
- ✅ Clean architecture with DTOs and controllers

## 🛠️ Tech Stack

- **.NET 9.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 9.0.15**
- **PostgreSQL** (via Npgsql 9.0.4)
- **Swagger/OpenAPI**

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- IDE: Visual Studio, Rider, or VS Code

## ⚙️ Setup & Installation

### 1. Clone the repository
```bash
git clone https://github.com/andjela-kostic/conferences.git
cd conferences
```

### 2. Configure Database Connection

Create `appsettings.Development.json` in the `Conferences2` folder with your database credentials:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ConferencesDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

> ⚠️ **Note:** `appsettings.Development.json` is gitignored to keep credentials safe.

### 3. Apply Database Migrations
```bash
cd Conferences2
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
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

Use the included `Conferences2.http` file with REST Client extension or similar tools to test all endpoints.