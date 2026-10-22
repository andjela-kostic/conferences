# Conference Management API

[![CI](https://github.com/andjela-kostic/conferences/actions/workflows/ci.yml/badge.svg)](https://github.com/andjela-kostic/conferences/actions/workflows/ci.yml)

A REST API for managing conferences and their topics, built with ASP.NET Core 10 and PostgreSQL. It uses a modular architecture with `Domain`, `Application`, `Infrastructure`, and `API` projects.

## 🚀 Features

- ✅ Full CRUD operations for conferences, topics, and users
- ✅ Many-to-many relationship between users and topics
- ✅ Topic registration and unregistration for conference attendees
- ✅ PostgreSQL database with Entity Framework Core
- ✅ OpenAPI document with [Scalar](https://scalar.com/) API reference (Development)
- ✅ Errors as [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) (`application/problem+json`)
- ✅ Request validation (DataAnnotations) backed by database constraints (column lengths, unique e-mail and registration)
- ✅ Health checks for liveness and readiness (database)
- ✅ Modular architecture with DTOs, services, repositories, and controllers

## 🛠️ Tech Stack

- **.NET 10 (LTS)** — SDK version pinned in `global.json`
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **PostgreSQL** (via Npgsql EF Core provider)
- **OpenAPI** (built-in `Microsoft.AspNetCore.OpenApi`) + **Scalar** UI

Package versions are managed centrally in `Directory.Packages.props`; shared build settings
(target framework, nullable, warnings as errors, code style) live in `Directory.Build.props`.

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (see `global.json` for the exact version)
- [Docker](https://docs.docker.com/get-docker/) (Docker Desktop, OrbStack, …) — runs PostgreSQL locally and is required by the integration tests.
  A locally installed [PostgreSQL](https://www.postgresql.org/download/) works too.
- IDE: Visual Studio, Rider, or VS Code

## ⚡ Quick start (Docker)

```bash
cp .env.example .env            # set POSTGRES_PASSWORD (and the ports if 5432/8080 are taken)
```

**Everything in Docker** (PostgreSQL + API built from source, migrations applied on start):

```bash
docker compose up -d --build --wait
curl http://localhost:8080/api/conferences
```

**Only the database in Docker**, API from the IDE / `dotnet run`:

```bash
docker compose up -d --wait db

dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=ConferencesDb;Username=postgres;Password=<POSTGRES_PASSWORD>" --project API

dotnet tool restore
dotnet ef database update --project Infrastructure --startup-project API
dotnet run --project API
```

Stop with `docker compose down` (add `-v` to also delete the data).
`docker-compose.override.yml` (local only) publishes the database port and builds the API from source;
`docker-compose.yml` on its own uses the published image and is what Azure uses (see [Deployment](#-deployment-azure-container-apps)).
The sections below explain each step in more detail.

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

> **Upgrading an existing database:** the `AddValidationConstraints` migration lowercases existing e-mail addresses and
> removes duplicate topic registrations before adding the unique indexes. It fails (and changes nothing) if two users
> end up with the same e-mail address or a text value is longer than the new limit — fix those rows and run it again.

### 4. Run the Application
```bash
dotnet run --project API/API.csproj
```

The API will be available at:
- HTTP: `http://localhost:5018`
- HTTPS: `https://localhost:7241` (local profile only; in containers TLS is terminated by the ingress)
- API reference (Scalar, Development only): `http://localhost:5018/scalar`
- OpenAPI document (Development only): `http://localhost:5018/openapi/v1.json`
- Health: `GET /health/live` (process is up) and `GET /health/ready` (database reachable)

### Errors

Errors are returned as ProblemDetails (`application/problem+json`):

| Status | When |
| --- | --- |
| `400` | Invalid request body — `ValidationProblemDetails` with an `errors` entry per invalid field (see [Validation](#validation)) |
| `404` | A resource in the route **or** referenced in the body (e.g. `conferenceId`, `userId`, `topicId`) does not exist |
| `409` | Conflicting state: the e-mail address is already used by another user, or the user is already registered for the topic |
| `500` | Unexpected error (no internal details are exposed) |

### Validation

| Field | Rules |
| --- | --- |
| Conference / topic `name` | required, max 200 characters |
| Conference / topic `description` | required, max 2000 characters |
| Conference `date` | required, **UTC only** — ISO 8601 ending in `Z` (e.g. `2027-06-01T09:00:00Z`); values without a time zone or with an offset are rejected |
| Topic `conferenceId`, registration `userId` / `topicId` | positive integer |
| User `firstName` / `lastName` | required, max 100 characters |
| User `email` | required, valid address, max 254 characters, **unique**; stored trimmed and in lowercase, so `Ana@Example.com` and `ana@example.com` are the same address |

The same lengths and uniqueness rules are enforced by the database (`varchar(n)` columns, unique indexes on
`Users.Email` and `UserTopics (UserId, TopicId)`), so concurrent requests cannot create duplicates either — they get `409`.

A topic belongs to exactly one conference: topics are created with `POST /api/topics` and moved to another conference
by changing `conferenceId` with `PUT /api/topics/{id}`. The conference endpoints do not accept topic IDs.

## 📚 API Endpoints

Responses are JSON DTOs (`ConferenceResponse`, `TopicResponse`, `UserResponse`), never database entities.
`POST` returns `201 Created` with a `Location` header pointing to the new resource; `PUT`/`DELETE` return `204 No Content`.
The full, always up-to-date contract (with descriptions) is in the OpenAPI document / Scalar UI.

### Conferences
- `GET /api/conferences` - Get all conferences (with their topics)
- `GET /api/conferences/{id}` - Get a conference (with its topics)
- `POST /api/conferences` - Create a new conference
- `PUT /api/conferences/{id}` - Update a conference
- `DELETE /api/conferences/{id}` - Delete a conference (and its topics)

### Topics
- `GET /api/topics` - Get all topics
- `GET /api/topics/{id}` - Get a topic
- `GET /api/topics/conference/{conferenceId}` - Get topics for a specific conference
- `POST /api/topics` - Create a new topic
- `PUT /api/topics/{id}` - Update a topic
- `DELETE /api/topics/{id}` - Delete a topic

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get a user
- `POST /api/users` - Register a new user
- `PUT /api/users/{id}` - Update a user
- `DELETE /api/users/{id}` - Delete a user (and their topic registrations)
- `POST /api/users/register-topic` - Register user to a topic (`204`, or `409` if already registered)
- `POST /api/users/unregister-topic` - Unregister user from a topic (`204`)
- `GET /api/users/{userId}/topics` - Get all topics a user is registered for
- `GET /api/users/topic/{topicId}` - Get all users registered for a topic

## 🧪 Testing

### Automated tests

| Project | What it covers | Needs |
| --- | --- | --- |
| `tests/Application.UnitTests` | Application services with in-memory fake repositories | nothing |
| `tests/API.IntegrationTests` | HTTP endpoints end-to-end (`WebApplicationFactory`) against a real PostgreSQL started by [Testcontainers](https://dotnet.testcontainers.org/) | a running Docker engine |

```bash
dotnet test --solution Conferences.sln                  # everything
dotnet test --project tests/Application.UnitTests       # unit tests only (no Docker)
```

The integration tests need no secrets: they run in the `Testing` environment against a throwaway
`postgres:17-alpine` container with a generated password, never your local database or user-secrets.

### Continuous integration

Two GitHub Actions workflows:

- **`ci.yml` (CI)** — every push to `main` and every pull request: restore → `dotnet format --verify-no-changes` →
  Release build (warnings are errors) → all tests; TRX results are uploaded as an artifact.
- **`docker.yml` (Docker image)** — runs after CI has **succeeded on `main`** (i.e. after a merge), never for pull requests:
  builds `API/Dockerfile` for `linux/amd64` from the tested commit and pushes it to `ghcr.io/andjela-kostic/conferences-api`
  with tags `latest` and `sha-<commit>`, authenticated with the built-in `GITHUB_TOKEN`.

### Manual requests

Use the included `API/Conferences.http` file with REST Client extension or similar tools to test all endpoints.

## ☁️ Deployment (Azure Container Apps)

`docker-compose.yml` (without the local override) describes the deployment: a `db` service (PostgreSQL 17)
and an `api` service (the image from GitHub Container Registry, listening on port 8080, applying migrations on start).

```bash
az containerapp compose create \
  --resource-group <rg> --environment <aca-environment> \
  --compose-file-path docker-compose.yml \
  --registry-server ghcr.io --registry-username <github-user> --registry-password <PAT with read:packages>
```

(The registry arguments are only needed while the GHCR package is private.)

**Secrets live in Container Apps, not in the repository or the image.** After the apps are created, move the
password and the connection string into Container App secrets and reference them from the environment variables:

```bash
az containerapp secret set -g <rg> -n db  --secrets postgres-password=<password>
az containerapp update     -g <rg> -n db  --set-env-vars POSTGRES_PASSWORD=secretref:postgres-password

az containerapp secret set -g <rg> -n api --secrets db-connection="Host=db;Port=5432;Database=ConferencesDb;Username=postgres;Password=<password>;Gss Encryption Mode=Disable"
az containerapp update     -g <rg> -n api --set-env-vars ConnectionStrings__DefaultConnection=secretref:db-connection
```

Things to check for the database container on Container Apps:

- **Persistence** — without a storage mount the data is lost when the `db` replica restarts. Mount an Azure Files share
  (NFS is recommended for PostgreSQL) at `/var/lib/postgresql/data`, and keep `db` at exactly one replica.
- **Networking** — the API reaches PostgreSQL through internal TCP ingress on `db` (port 5432); it must not be exposed externally.
- For production data, Azure Database for PostgreSQL Flexible Server (backups, HA) is the safer option —
  only `ConnectionStrings__DefaultConnection` would change.

Health probes for the `api` container app (Container App → Containers → Health probes):

| Probe | Path | Port |
| --- | --- | --- |
| Liveness | `/health/live` | 8080 |
| Readiness | `/health/ready` | 8080 |
| Startup (optional) | `/health/live` | 8080 |

Readiness takes a replica out of rotation while the database is unreachable; liveness does not check the
database, so a database outage does not cause the API to be restarted in a loop.

