# Project Context — Support Desk Technical Assessment

## How I want you to help me

I am building this myself. **Do not write the solution for me.** I am using you to get oriented in an unfamiliar ecosystem, not to generate code I will submit without understanding.

What I want from you:

- Explain concepts and conventions when I ask
- Show me minimal, illustrative snippets that demonstrate a pattern, which I will then apply myself
- Answer "how does X work in .NET/Angular" and "what is the idiomatic way to do Y"
- Review code I have written and tell me what is wrong or unidiomatic
- Help me debug errors I hit
- Push back if my design decisions are poor, and explain why

What I do not want:

- Complete files or whole features written for me
- Long code dumps I have not asked for
- Being handed the answer when I am working through something

If I ask for something that crosses into doing the work for me, say so and redirect.

## My background

**Current role:** Full-stack software engineer, 8 months professional experience.

**Comfortable with:**
- Python — Django, Django REST Framework, FastAPI
- TypeScript and JavaScript — Next.js, Vue 3
- REST API design, DTOs/serializers, ORM and migrations
- PostgreSQL, Redis, Celery
- Docker, Kubernetes, ArgoCD, Git
- OAuth 2.0, JWT authentication
- Testing with Playwright, Selenium, Postman

**University-level only, rusty:** Java, C++

**No experience at all:**
- C#
- ASP.NET Core
- Entity Framework Core
- Angular
- RxJS
- SQL Server
- xUnit / NUnit / Jasmine / Karma

Assume I understand backend architecture, ORMs, dependency injection as a concept, and typed languages. I need the .NET and Angular specifics, not the fundamentals.

Useful mappings for explaining things to me:

| .NET / Angular | My equivalent |
|---|---|
| ASP.NET Core Controller | Django view / DRF ViewSet |
| Entity Framework Core | Django ORM |
| EF Core migrations | Django migrations |
| DTO | DRF serializer |
| Angular Service | Vue composable / API module |
| RxJS Observable | Promise, but a stream |

## Constraints

- **48-hour deadline** from receiving the assignment
- I am learning both stacks from zero during that window
- Full completion is unlikely — I am optimising for demonstrated judgment over feature coverage
- Local development only

## Priority order

If time runs short, this is the order things matter:

1. Domain layer and business rules, correctly placed
2. Backend unit tests covering status transitions and due date calculation
3. README explaining design decisions
4. Core ticket endpoints with DTOs and consistent error handling
5. Angular ticket list and detail pages
6. Everything else

The assignment states explicitly that the evaluation is about whether business rules are enforced correctly and in the right place, not about CRUD completeness. A strong backend with a thin frontend beats half-finished everything.

---

# The Assignment

## Purpose

Build a small but complete feature across the full stack. The evaluators are looking for clean, readable, well-reasoned code and correct handling of business rules — not a finished product.

## Scenario

An internal tool called **Support Desk**, used by a support team to track customer issues from report to closure.

## Data model

**Agent**
- Id
- Full name
- Email (unique)
- Department — Technical / Billing / General
- Active (bool)

**Ticket**
- Id
- Reference — auto-generated, unique, human-readable, e.g. `TCK-2026-0001`
- Title
- Description
- Customer name
- Customer email
- Priority — Low / Normal / High / Critical
- Status — New / In Progress / Resolved / Closed
- Assigned agent (optional; new tickets start unassigned)
- Created date
- Last modified date
- Resolved date
- Closed date
- Due date — **calculated by the system, never sent by the client**

**Comment**
- Id
- Ticket
- Author name
- Body
- Created date

A ticket has many comments. An agent can be assigned many tickets.

## Business rules

These must be enforced on the **backend**. The frontend may reflect them for better UX, but the API must never rely on the client to do so.

1. **Due date** is derived from priority at creation time:
   - Critical → 4 hours after creation
   - High → 1 day
   - Normal → 3 days
   - Low → 7 days

   If priority changes while the ticket is still open, the due date is recalculated from the **original creation date**.

2. **Allowed status transitions:**
   - New → In Progress
   - In Progress → Resolved
   - Resolved → Closed
   - Resolved → In Progress (reopening)

   Any other transition must be rejected with a clear error. A ticket cannot skip from New to Resolved. A Closed ticket can never be reopened.

3. A ticket cannot move to **In Progress** unless an **active** agent is assigned to it.

4. An **inactive** agent cannot be assigned to any ticket.

5. A **Closed** ticket is read-only — no edits, no status changes, no new comments.

6. Resolved date and closed date are set by the system when the transition happens, not by the client.

7. A ticket is **overdue** when its due date has passed and its status is neither Resolved nor Closed.

## Backend — ASP.NET Core Web API

1. RESTful API using **C# and ASP.NET Core**, with **Entity Framework Core** for data access.

2. Ticket endpoints:
   - List tickets with **pagination**, **search** (by reference, title, or customer name), and **filters** by status, priority, assigned agent, and overdue-only
   - Get a single ticket including its comments
   - Create a ticket, update its editable fields, delete it
   - Assign or unassign an agent
   - Change status — a dedicated endpoint rather than a generic update is preferred; explain the choice in the README
   - Add a comment to a ticket

3. Agent endpoints: list (with search) and get by id. Full CRUD optional.

4. Server-side validation with meaningful, machine-readable error responses. A rejected status transition must tell the caller **why**.

5. Consistent error handling — clients never receive a raw stack trace.

6. Use **DTOs**; do not expose EF entities directly through the API.

7. Database: **SQL Server** (LocalDB is fine) or **PostgreSQL**. Include EF Core migrations and a **seed dataset** — roughly 5 agents and 20 tickets across different statuses and priorities, including a few overdue — so the app is usable immediately after setup.

## Frontend — Angular

1. **Ticket list page**
   - Table or card list showing reference, title, customer, priority, status, assigned agent, due date
   - Overdue tickets visually highlighted
   - Search and filters wired to the API — **server-side**, not client-side filtering
   - Working pagination, plus loading and empty states

2. **Ticket detail page**
   - Full ticket information and comment thread, with a form to add a comment
   - Status change action that only offers the transitions currently allowed
   - Agent assignment
   - Everything disabled once the ticket is closed

3. **Create / edit ticket page or dialog**
   - **Reactive Forms** with client-side validation and clear messages
   - Server-side validation and rule violations surfaced readably

4. Delete with a confirmation step.

5. HTTP calls isolated in **services** — no direct `HttpClient` usage inside components.

6. Reasonable use of **RxJS**, for example a debounced search.

## Quality requirements

1. At least **4 meaningful backend unit tests**, covering status transition rules and due date calculation.
2. At least **2 frontend unit tests** (Jasmine/Karma or Jest) for a service or component.
3. A **README.md** covering:
   - How to run backend and frontend locally, including database setup
   - Where the business rules were placed and why
   - Any assumptions made
   - What would be improved or added with more time
   - Roughly how long the assignment took

## Submission

`.zip` archive named `FirstName_LastName_FullStack.zip`, sent by email reply within 48 hours of receipt.

---

# Design decisions I want to get right

These are the things the assignment is actually grading, and where I want your input most:

- **Where the business rules live.** They should sit in a domain layer that knows nothing about HTTP or the database. If I can test a rule without spinning up a web server or a DB, it is in the right place.
- **Status transitions as data, not scattered conditionals.** A map of current status to permitted next statuses, validated in one place.
- **Why a dedicated status endpoint** rather than a generic PATCH — a state transition has its own preconditions and side effects, and flattening it into a field update loses that.
- **DTOs separating the API contract from the database schema**, so schema changes are not automatically breaking API changes.
- **Global exception handling in middleware** so no raw stack traces ever reach a client.

Tell me if any of this reasoning is wrong or incomplete for the .NET ecosystem specifically.
