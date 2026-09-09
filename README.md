# Support Desk

Name  : Luqman Hakim bin Noorazmi
Email : luq3973@gmail.com
Phone : 013-287 4100
Date  : 09/09/2026

Internal tool for tracking customer support tickets from report to closure.
ASP.NET Core Web API (.NET 8) + Entity Framework Core + PostgreSQL, with an Angular 18 frontend.

---

## How to run (Local)

### Prerequisites

- .NET 8 SDK
- Node 20+
- PostgreSQL 14+
- EF Core CLI, pinned to 8.x to match the runtime packages:
  ```
  dotnet tool install --global dotnet-ef --version '8.*'
  ```

### DB

Create an empty database, then let migrations build the schema:

```
createdb supportdesk
```

Connection string lives in `src/SupportDesk.Api/appsettings.Development.json`:

```
Host=localhost;Port=5433;Database=supportdesk;Username=dimex
```

> **Note on the port.** This was developed against PostgreSQL on port **5433**, because
> port 5432 was already taken by an unrelated container on the dev machine. If your
> Postgres is on the default port, change `Port=5433` to `Port=5432` in
> `appsettings.Development.json` before running migrations.

Apply migrations and seed data:

```
dotnet ef database update -p src/SupportDesk.Infrastructure -s src/SupportDesk.Api
```

The seed data is applied automatically and includes 5 agents and 20 tickets across
different statuses and priorities, including several overdue ones.

### Backend

```
dotnet run --project src/SupportDesk.Api
```

- API: http://localhost:5116
- Swagger UI: http://localhost:5116/swagger

### Frontend

```
cd frontend
npm install
npm start
```

- App: http://localhost:4200

### Tests

```
dotnet test                      # backend
cd frontend && npx ng test       # frontend
```

---

## Where the business rules live, and why

All business rules are in **`SupportDesk.Domain`**, which has **no package references at all** —
no Entity Framework, no ASP.NET Core, nothing. This is deliberate and it is the main
structural decision in the solution.

The project layout is three layers, with dependencies pointing inward only:

```
SupportDesk.Api             →  HTTP: controllers, DTOs, exception handling
        ↓
SupportDesk.Infrastructure  →  Persistence: DbContext, EF configuration, migrations
        ↓
SupportDesk.Domain          →  Business rules. Depends on nothing.
        ↑
SupportDesk.Domain.Tests    →  Tests the rules with no database and no web host
```

The test for whether a rule is in the right place: **if it can be tested without starting a
web server or a database, it is in the right place.** Every rule below meets that test —
`SupportDesk.Domain.Tests` references only `SupportDesk.Domain`.

Because `SupportDesk.Domain.csproj` has an empty `<ItemGroup>`, this separation is enforced
by the compiler rather than by convention. EF Core cannot leak into the domain by accident.

### Rule placement

| Rule | Where | Notes |
|---|---|---|
| 1. Due date from priority | `TicketRules.CalculateDueDate` | Always calculated from the original creation date, so changing priority on an open ticket recalculates rather than restarts the clock |
| 2. Allowed status transitions | `TicketRules.AllowedFrom` | One method, one switch — the single authority on what is legal |
| 3. In Progress requires an active agent | `Ticket.ChangeStatus` | Needs the agent's state, so it is checked where both objects are available |
| 4. Inactive agents cannot be assigned | `Ticket.Assign` | |
| 5. Closed tickets are read-only | `Ticket` | Guard at the top of every mutating method |
| 6. Resolved/closed dates set by system | `Ticket.ChangeStatus` | Never accepted from the client |
| 7. Overdue calculation | `Ticket` | Due date passed and status is neither Resolved nor Closed |

Controllers parse HTTP and map DTOs. They do not decide whether an operation is legal —
they call a domain method, and that method either succeeds or throws.

### Why a dedicated status endpoint instead of a generic PATCH

A status change is a **state transition**, not a field update. It has its own preconditions
(is this transition legal? is there an active agent?) and its own side effects (stamping
`ResolvedAt` / `ClosedAt`). Flattening it into a generic update would mean:

- The API contract stops expressing that only certain transitions are valid
- Rejecting an illegal transition returns the same shape as any other validation error,
  so the caller cannot tell *why* it was rejected
- A client could send status alongside unrelated fields, making the request's intent unclear

A dedicated `POST /api/tickets/{id}/status` endpoint makes the operation explicit, gives it
its own request shape, and lets it return a specific error explaining the rejected transition.

### Errors

Rule violations throw a domain exception carrying a machine-readable reason. A global
exception handler translates these into RFC 7807 `ProblemDetails` responses — a rejected
transition returns **409 Conflict** with an explanation of why it was rejected. Clients never
receive a stack trace.

---

## Assumptions

- **`Comment.AuthorName` is free text, not a foreign key to `Agent`.** The assignment's data
  model specifies "Author name" rather than an agent reference. This also allows comments from
  customers, not just agents, and preserves the author's name as it was at the time of writing
  even if an agent is later renamed or removed.
- **No `Customer` entity.** The specification models customer name and email as fields on the
  ticket, so customers are not stored separately.
- **SLA durations are desk policy, not a property of the priority.** `TicketPriority` is a plain
  enum; the durations live in `TicketRules` alongside the other rules. "High" does not
  inherently mean one day — the support desk decided it does, and that policy could change.
- **Enums are stored as strings in the database** rather than integers, so that reordering an
  enum member can never silently reinterpret existing rows.
- **All timestamps are UTC.** PostgreSQL `timestamptz` via Npgsql requires it, and it avoids
  ambiguity around the due-date calculation.
- **The list of currently allowed transitions is computed on the server** and returned as part
  of the ticket response, so the UI can offer only legal actions without duplicating the rules.
- **No authentication.** Not requested, and out of scope for the time available.

## Design decisions

- **No generic repository over EF Core.** `DbContext` is already a Unit of Work and `DbSet<T>`
  is already a repository. Wrapping them adds indirection and re-implements querying badly.
- **No MediatR / CQRS, no AutoMapper.** At this size they add configuration and concepts
  without reducing real complexity. DTO mapping is done explicitly so it is visible.
- **Exceptions rather than a Result type** for rule violations. A Result type is arguably
  cleaner, but it affects every method signature. Exceptions plus one global handler was the
  better trade for the scope.
- **DTOs at the API boundary.** EF entities are never returned directly, so a schema change is
  not automatically a breaking API change.
- **`switch` over if/else chains** for the rule methods, so each enum member appears explicitly —
  `case TicketStatus.Closed:` documents that the terminal state was considered rather than
  forgotten.

---

## What I would improve or add with more time

- Authentication and authorisation, with the acting agent taken from the token rather than the
  request body.
- Integration tests covering the endpoints end to end, in addition to the domain unit tests.
- A `docker-compose.yaml` for the database so setup is one command and does not depend on a
  local Postgres install or a particular port.
- Optimistic concurrency on tickets, so two agents changing status simultaneously cannot
  overwrite one another.
- Structured logging and correlation IDs.
- Pagination metadata on the frontend (page size selection, jump to page).
- Better accessibility on the Angular pages — proper focus management in dialogs and ARIA
  labelling on the status controls.

## Time taken

_TBC_
