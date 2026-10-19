# Document Management System (DMS)

DMS is a document management system we are building as a semester project. Users can register, log in, upload documents and search them. The backend is an ASP.NET Core REST API (.NET 10) with PostgreSQL, the frontend is a React app. After every upload a message is sent to RabbitMQ so the document can be processed by an OCR worker later on.

## Features

- Register and log in (JWT)
- Upload one or more files at once
- List, search (by name) and delete documents
- Look up users by ID, username or email
- Swagger UI for the API
- Share requests and document permissions are already implemented in the business layer, but not yet available through the API
- OCR worker is connected to RabbitMQ, but the actual OCR part is still missing

## Tech stack

- Backend: ASP.NET Core, Entity Framework Core (Npgsql), FluentValidation, Mapster, MassTransit
- Database: PostgreSQL 17
- Messaging: RabbitMQ 4
- Frontend: React 19 with TypeScript, Vite, Tailwind CSS and React Router, served by nginx
- Tests: NUnit, Moq, MockQueryable
- CI: GitHub Actions

Everything runs in Docker Compose.

## Architecture

The backend is split into layers. Each project only knows about the layer below it.

- `DMS.Web` is the React frontend. nginx serves it and forwards everything under `/api/*` to the API.
- `DMS.API` contains the controllers, JWT authentication, a validation filter and the global exception handler. All dependencies are registered in `Program.cs`.
- `DMS.Business` contains the services (`DocumentService`, `UserService`, `ShareRequestService`, `UserDocumentService`) and the `UnitOfWork`.
- `DMS.Business.Models` holds the DTOs, validators, the Mapster config and the RabbitMQ message and consumer.
- `DMS.DAL` holds the `DmsDbContext`, the entities and the EF Core configurations.
- `DMS.API.Test`, `DMS.Business.Test` and `DMS.DAL.Test` are the test projects.

```mermaid
flowchart LR
    Browser --> Web["DMS.Web (nginx + React)"]
    Web -- "/api/*" --> API[DMS.API]
    API --> Business[DMS.Business]
    Business --> DAL[DMS.DAL]
    DAL --> DB[(PostgreSQL)]
    API -- ProcessOcrDocument --> MQ[[RabbitMQ]]
    MQ --> OCR[OcrWorkerConsumer]
```

A typical request goes like this: the controller gets a DTO, the `ValidationFilter` validates it, the service does the actual work on the entities and at the end `UnitOfWork.SaveChangesAsync()` saves everything in one go. The result is mapped back to a DTO with Mapster. If something goes wrong, the service throws an exception and the `GlobalExceptionHandler` turns it into a `ProblemDetails` response.

### Database

```mermaid
erDiagram
    USER ||--o{ USER_DOCUMENT : has
    DOCUMENT ||--o{ USER_DOCUMENT : "accessed by"
    USER ||--o{ SHARE_REQUEST : owns
    USER ||--o{ SHARE_REQUEST : receives
    DOCUMENT ||--o{ SHARE_REQUEST : "shared in"
```

`UserDocument` is the join table between users and documents and stores the permission (`Owner` or `User`). We designed the schema in drawSQL, the export (`drawSQL-pgsql-export-2026-09-24.sql`) is loaded automatically when the database container starts for the first time.

### Upload

```mermaid
sequenceDiagram
    actor U as User
    participant A as DocumentController
    participant S as DocumentService
    participant Q as RabbitMQ
    participant O as OcrWorkerConsumer

    U->>A: POST /Document/upload
    loop each file
        A->>S: AddForOwnerAsync
        A->>Q: publish ProcessOcrDocument
    end
    A->>A: SaveChangesAsync
    A-->>U: list of DocumentDto
    Q->>O: consume (async)
```

## Design decisions

We use DTOs in the API and never return entities directly. That way things like the password hash or the internal IDs never end up in a response.

Every entity has an internal `int Id` for the database and a `Guid EntityId` (UUID v7) that is used to the outside. At first we only had int IDs, but they are easy to guess (`/Document/1`, `/Document/2`, ...), so we switched to GUIDs. Version 7 is time based, which works better for indexes than random GUIDs.

The Unit of Work makes sure that, for example, a multi-file upload is saved in a single transaction instead of one save per file.

Validation is done with FluentValidation. A global filter runs the validators before the controller action, so the controllers stay small.

For errors there is one global exception handler. The services just throw exceptions and the handler maps them to status codes:

- `ValidationException` and `ArgumentException` → 400
- `UnauthorizedAccessException` → 401
- `KeyNotFoundException` → 404
- everything else → 500

Login uses JWT because the frontend is a separate app and we didn't want server-side sessions. Passwords are hashed with PBKDF2 (SHA-256, 600,000 iterations) from the .NET crypto library, so no extra package was needed. The hash has a version prefix, which means we can change the parameters later and old hashes get updated on the next login.

OCR takes time, so it shouldn't block the upload. That's why the API only publishes a message to RabbitMQ (via MassTransit) and the consumer handles it in the background.

Because nginx proxies `/api` to the backend, frontend and API run under the same origin and we don't need any CORS setup.

Configuration like connection strings and passwords is passed in through env files and is not committed to the repo.

## Getting started

You need Docker and Docker Compose. If you want to run it without Docker, you need the .NET 10 SDK, Node.js 24 and a PostgreSQL 17 instance.

### Env file

Copy [`.env.example`](.env.example) and create a `.env.local` in the root folder:

```ini
COMPOSE_FILE=local-compose.yml
POSTGRES_DB=dms
POSTGRES_USER=dms
POSTGRES_PASSWORD=dms_password
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=dms;Username=dms;Password=dms_password
RABBITMQ_DEFAULT_USER=rabbitmq
RABBITMQ_DEFAULT_PASS=rabbitmq_password
ConnectionStrings__RabbitMq=amqp://rabbitmq:rabbitmq_password@rabbitmq:5672
```

`local-compose.yml` reads `.env.local`, `docker-compose.yml` reads `.env`.

### Start

```bash
docker compose -f local-compose.yml up --build
```

Add `-d` if you want it to run in the background.

After that the following is available:

| Service | URL / Port |
|---|---|
| Web UI | http://localhost |
| API | http://localhost:8080 |
| Swagger UI (Development only) | http://localhost:8080/swagger |
| PostgreSQL | localhost:5432 |
| RabbitMQ | localhost:5672 |
| RabbitMQ Management | http://localhost:15672 |

### Stop / reset

```bash
docker compose -f local-compose.yml down      # stop
docker compose -f local-compose.yml down -v   # stop and delete the database volume
```

After `down -v` the schema is created again on the next start.

### Endpoints

| Method | Route | Login needed |
|---|---|---|
| POST | `/User/register` | no |
| POST | `/User/login` | no |
| GET | `/User/me` | yes |
| GET | `/User`, `/User/{id}` | yes |
| GET | `/User/by-username/{username}`, `/User/by-email/{email}` | yes |
| POST | `/Document/upload` | yes |
| GET | `/Document`, `/Document/{id}` | yes |
| GET | `/Document/search` | yes |
| DELETE | `/Document/{id}` | yes |

The full spec is in [`DMS.API/api-spec.yaml`](DMS.API/api-spec.yaml). For manual testing there is a Postman collection: [`DMS.postman_collection.json`](DMS.postman_collection.json).

## Testing

We mainly write unit tests with NUnit. Dependencies are mocked with Moq. To mock the EF Core `DbSet`s (including async queries) we use MockQueryable, so the tests don't need a real database.

There is one test project per layer:

- `DMS.API.Test` tests the `DocumentController` and `UserController`, e.g. if the right status codes come back and if the services are called correctly.
- `DMS.Business.Test` tests the services and their rules, for example that you can't share a document with yourself, that a user can't get access to the same document twice, or that an error is thrown for unknown users.
- `DMS.Business.Test/DtoValidatorTests` checks the validators with valid and invalid input.
- `DMS.DAL.Test` exists but has no tests yet.

Run all tests with:

```bash
dotnet test DMS.slnx
```

Besides that we test the API by hand with Swagger and Postman and click through the web UI.

The CI pipeline ([`.github/workflows/ci.yaml`](.github/workflows/ci.yaml)) runs on every push and pull request to `main`. It builds the backend and runs the tests, and in a second job it builds the frontend. The `ci-complete` job only passes when both jobs passed, so we only need one required check in the branch protection rules.

## Lessons learned

- Switching from int IDs to GUIDs late in the project meant changing controllers, services, DTOs and tests at the same time. Next time we would decide this at the start.
- At the beginning, config and secrets were spread between `appsettings` and the compose file. With the env files it's now in one place and it's clear what is local and what isn't.
- Mocking EF Core is harder than expected. Async LINQ queries didn't work with plain Moq, we needed MockQueryable for that.
- Without a health check in Docker Compose the API sometimes started before RabbitMQ or Postgres were ready and crashed.
- Small feature branches with pull requests worked well for us. Reviews were faster and we had fewer merge conflicts.
- The CI caught a few broken builds and failing tests before they ended up on `main`.

## Tracked work

We work with feature branches and merge everything into `main` through pull requests once the CI is green.

Our team's hours spent:
- Birdi Rohit: ~39
- Kainz Leopold: ~41
- Mikulovic Luka: ~45


What has been done so far:

- Docker Compose setup and env example (#8)
- Test projects (#9)
- CI pipeline (#10)
- DAL and Unit of Work (#11)
- DTOs and mapping (#12)
- Controllers, services and Postman collection (#13, #14)
- Unit tests for API, Business and DAL (#15)
- First frontend design (#22)
- DTO validation and GUIDs instead of int IDs (#23)
- JWT authentication with login/register in the frontend (#24)
- Logging, dashboard improvements and more tests (#25)
- Exception handling, frontend and business layer changes (#26)
- RabbitMQ/MassTransit for OCR, env files, frontend build in CI (#28)

Still open:

- actual OCR processing in `OcrWorkerConsumer` (right now it only logs)
- storing the file content (right now only metadata is saved)
- share requests in the API and the UI
- frontend tests and the test step in the CI
- tests for `DMS.DAL.Test`
