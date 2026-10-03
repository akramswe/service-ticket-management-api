# Service Ticket Management API

A small but complete C# web API for managing software support tickets. The project demonstrates object-oriented design, REST API development, validation, database persistence, testing, documentation, and maintenance-oriented workflows.

## Tech stack

- C# / .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- xUnit.net v3
- OpenAPI
- Docker
- GitHub Actions

## Features

- Create, read, update, and delete support tickets
- Assign ticket priority and an optional assignee
- Move tickets through a controlled status workflow
- Add support comments to a ticket
- Filter by status, priority, or assignee
- Search ticket titles and descriptions
- View simple ticket statistics
- Validate incoming request models
- Return Problem Details responses for application errors
- Persist data to SQLite
- Seed sample data on first start
- Unit tests for the service layer
- CI workflow for restore, build, and tests

## Ticket workflow

The API uses the following status transitions:

| Current status | Allowed next status |
| --- | --- |
| New | InProgress, Closed |
| InProgress | Resolved, Closed |
| Resolved | InProgress, Closed |
| Closed | None |

Trying an invalid transition returns HTTP `409 Conflict`.

## Project structure

```text
service-ticket-management-api/
├── .github/workflows/ci.yml
├── src/
│   └── ServiceTicketManagement.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── Domain/
│       ├── DTOs/
│       ├── Infrastructure/
│       ├── Services/
│       ├── Dockerfile
│       ├── Program.cs
│       └── ServiceTicketManagement.Api.csproj
├── tests/
│   └── ServiceTicketManagement.Tests/
├── ServiceTicketManagement.sln
└── README.md
```

The main request flow is:

```text
HTTP request -> TicketsController -> ITicketService/TicketService -> AppDbContext -> SQLite
```

This keeps HTTP concerns, business rules, and persistence logic separated while staying small enough to understand easily.

## Requirements

Install the .NET 10 SDK.

Check your installation:

```bash
dotnet --version
```

## Run locally

From the repository root:

```bash
dotnet restore ServiceTicketManagement.sln
dotnet run --project src/ServiceTicketManagement.Api
```

The HTTP profile uses:

```text
http://localhost:5168
```

On the first start, the application automatically creates `service-tickets.db` and adds a few sample tickets.

## OpenAPI

When the API is running in the Development environment, the generated OpenAPI document is available at:

```text
http://localhost:5168/openapi/v1.json
```

The repository also contains `ServiceTicketManagement.Api.http`, which can be used from Visual Studio, Rider, or VS Code REST Client-style tooling to call the endpoints.

## API endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/tickets` | List and filter tickets |
| GET | `/api/tickets/{id}` | Get one ticket |
| GET | `/api/tickets/statistics` | Get ticket counts |
| POST | `/api/tickets` | Create a ticket |
| PUT | `/api/tickets/{id}` | Update ticket details |
| PATCH | `/api/tickets/{id}/status` | Change ticket status |
| POST | `/api/tickets/{id}/comments` | Add a support comment |
| DELETE | `/api/tickets/{id}` | Delete a ticket |

### Filters

`GET /api/tickets` accepts these optional query parameters:

- `status`
- `priority`
- `assignee`
- `search`

Example:

```http
GET /api/tickets?status=InProgress&priority=High
```

## Example: create a ticket

```http
POST /api/tickets
Content-Type: application/json

{
  "title": "Customer portal login issue",
  "description": "Investigate why a user receives an unexpected error after login.",
  "priority": "High",
  "assignee": "Application Support"
}
```

A newly created ticket always starts with status `New`.

## Example: update ticket status

```http
PATCH /api/tickets/{ticketId}/status
Content-Type: application/json

{
  "status": "InProgress"
}
```

## Example: add a comment

```http
POST /api/tickets/{ticketId}/comments
Content-Type: application/json

{
  "author": "Washim Akram",
  "message": "Issue reproduced locally. Checking logs and database state."
}
```

## Run tests

```bash
dotnet test ServiceTicketManagement.sln
```

The test suite covers:

- ticket creation
- status workflow rules
- rejection of invalid status changes
- priority filtering
- comment creation
- ticket statistics

## Run with Docker

Build the image from the API project directory:

```bash
cd src/ServiceTicketManagement.Api
docker build -t service-ticket-management-api .
```

Run it:

```bash
docker run --rm -p 8080:8080 service-ticket-management-api
```

Then call:

```text
http://localhost:8080/api/tickets
```

## Design decisions

### Controller and service separation

`TicketsController` handles HTTP requests and response codes. `TicketService` contains ticket workflow and application logic. `AppDbContext` handles persistence with Entity Framework Core.

### DTOs instead of exposing entities

Request and response DTOs keep the public API contract separate from the database entities. This makes the API easier to change without directly coupling clients to persistence models.

### Controlled status changes

Status changes are validated in the service layer. Invalid transitions raise an application exception that is converted into a `409 Conflict` Problem Details response by the global exception handler.

### SQLite for local development

SQLite keeps the project easy to clone and run without requiring a separate database server. The EF Core layer can later be changed to SQL Server or PostgreSQL with limited impact on the rest of the application.

## Possible next steps

For a production system, useful extensions would include authentication and authorization, pagination, audit history, optimistic concurrency, database migrations, structured application metrics, and integration tests.
