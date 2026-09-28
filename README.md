# Document Management System (DMS)

A RESTful Document Management System built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **PostgreSQL 17**.

---

## Features

- **Document Management:** Multi-file upload, retrieval, search by name, and deletion.
- **User Management:** Registration and user lookup by ID, username, or email.
- **OpenAPI / Swagger UI:** API documentation.

---

## Prerequisites

- Docker & Docker Compose
- *(Optional for local development without Docker)* .NET 10 SDK & PostgreSQL 17

---

## Environment Configuration

The application and database configuration relies on environment variables. You can copy the template from [`.env.example`](file:///home/leopold/FH/SEM%205/SWEN/.env.example).

### Create `.env.local`
In the project root directory, create a `.env.local` file for local docker-compose execution:

```ini
COMPOSE_FILE=local-compose.yml
POSTGRES_DB=dms
POSTGRES_USER=dms
POSTGRES_PASSWORD=dms_password
ASPNETCORE_ENVIRONMENT=Development
```

> **Note:**
> - `local-compose.yml` uses `.env.local`.
> - `docker-compose.yml` uses `.env`.

---

## Running with Local Compose

### 1. Build and Start Services
From the root directory, run:

```bash
docker compose --env-file .env.local -f local-compose.yml up --build
```
*(Add `-d` to run in detached/background mode)*

The database will initialize automatically using `drawSQL-pgsql-export-2026-09-24.sql` on its first startup.

### 2. Available Endpoints & Services

| Service | URL / Port | Description |
|---|---|---|
| **API** | `http://localhost:8080` | DMS REST API |
| **Swagger UI** | `http://localhost:8080/swagger` | Interactive API documentation |
| **PostgreSQL** | `localhost:5432` | Database instance |

---

## Stopping & Resetting

- **Stop containers:**
  ```bash
  docker compose --env-file .env.local -f local-compose.yml down
  ```

- **Reset database (removes volume data and triggers fresh schema initialization on next start):**
  ```bash
  docker compose --env-file .env.local -f local-compose.yml down -v
  ```
