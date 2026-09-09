
# Support Desk

- Name        : Luqman Hakim bin Noorazmi
- Email       : luq3973@gmail.com
- Phone       : 013-287 4100
- Date        : 09/09/2026


---

## How to run it

You need .NET 8, Node 20 or newer, and PostgreSQL.

### 1. Database

Create an empty database:

```
createdb supportdesk
```

The connection string is in `src/SupportDesk.Api/appsettings.Development.json`:

```
Host=localhost;Port=5433;Database=supportdesk;Username=dimex
```

I built this against PostgreSQL on port 5433, because port 5432 was already
taken on my machine. If your PostgreSQL runs on the normal port change
`Port=5433` to `Port=5432` before you continue. You may also need to change
the username to your own.

Install the database tool and create the tables:

```
dotnet tool install --global dotnet-ef --version '8.*'
dotnet ef database update -p src/SupportDesk.Infrastructure -s src/SupportDesk.Api
```

### 2. Backend

```
dotnet run --project src/SupportDesk.Api
```

The API runs at http://localhost:5116 and Swagger is at http://localhost:5116/swagger.

The first time it starts it fills the database with sample data: 5 agents and
20 tickets. The tickets cover every status and priority, and six of them are
already overdue so you can see the highlighting straight away.

### 3. Frontend

```
cd frontend
npm install
npm start
```

The app runs at http://localhost:4200. Leave the backend running as well.

### 4. Tests

```
dotnet test                      # 4 backend tests
cd frontend && npx ng test       # 2 frontend tests
```

---

## Where I put the business rules

All seven rules are in one project called `SupportDesk.Domain`.

That project does not know about the database or the web. It has no other code
attached to it at all. Everything else is built on top of it:

```
SupportDesk.Api             the web endpoints
        |
SupportDesk.Infrastructure  saves and loads from the database
        |
SupportDesk.Domain          the rules
```

I used a simple test to decide where a rule belongs. If I can test the rule
without starting a website or a database, it is in the right place. All seven
rules pass that test. The test project only knows about the rules project, and
the tests run in a few milliseconds.

The main reason I did it this way is that rules get lost when they are spread
around. If the rule for changing a status lives inside a web endpoint, then the
next person who adds a second endpoint has to remember to copy it. Keeping the
rules in one place means there is only ever one copy.

### Each rule and where it lives

| Rule | Where |
|---|---|
| 1. Due date comes from the priority | `TicketRules.CalculateDueDate` |
| 2. Only certain status changes are allowed | `TicketRules.AllowedFrom` |
| 3. A ticket needs an active agent before work starts | `Ticket.ChangeStatus` |
| 4. An inactive agent cannot be assigned | `Ticket.Assign` |
| 5. A closed ticket cannot be changed | Checked at the start of every method on `Ticket` |
| 6. The system sets the resolved and closed dates | `Ticket.ChangeStatus` |
| 7. A ticket is overdue if it is late and not finished | `Ticket.IsOverdue` |

The web endpoints do not decide anything. They read the request, call a method
on the ticket, and return the result. If a rule is broken, the ticket refuses
and the endpoint turns that into an error message.

### Why status has its own endpoint

Changing a status is not the same as editing a field.

It has its own conditions. You cannot move to In Progress without an active
agent. You cannot move a closed ticket at all. It also has its own side
effects, because the system has to record the date when a ticket is resolved
or closed.

If I had allowed status to be edited like any other field, all of that would
be hidden inside a general update. The caller would also get the same vague
error for a rejected status change as for a bad email address. A separate
endpoint keeps the operation obvious and lets it return a clear reason when
it says no.

### Error messages

When a rule is broken, the API returns a 409 with a sentence explaining what
went wrong. For example: "cannot change status from New to Resolved".

Bad input, like a missing title or an invalid email, returns a 400 listing the
fields that are wrong.

The user never sees a technical error page. Anything unexpected returns a plain
message and the details go to the server log instead.

---

## Assumptions I made

The task did not cover these, so I made a decision and wrote down why.

**Reopening a ticket clears the resolved date.**
The rules say the system owns the resolved date. If a ticket goes back to In
Progress, it is not resolved any more, so keeping the old date would be
misleading. I clear it.

**The comment author is just a name, not a link to an agent.**
The task lists "author name" for a comment. I also think customers reply on
their own tickets, not only agents, so tying it to an agent record would block
that. Keeping the name also means the comment still reads correctly if an agent
later leaves.

**A closed ticket can still be deleted.**
Rule 5 says a closed ticket cannot be edited. Deleting is not editing, and the
task asks for delete with a confirmation step, so I allowed it.

**The due date times are a support desk policy, not part of the priority.**
"High" does not automatically mean one day. The support desk decided that it
does, and they could change it later. So I kept those times with the other
rules rather than attaching them to the priority itself.

**The ticket reference is created by the API.**
Making `TCK-2026-0001` means checking which references already exist, and that
needs the database. The rules project is not allowed to touch the database, so
the API creates the reference and hands it over.

**The server tells the frontend which status changes are allowed.**
Every ticket response includes a list of the status changes that are currently
possible. The frontend draws one button per item in that list. This means the
frontend never contains a copy of the rules, and it can never offer a button
that the server would reject.

---

## About the tests

There are 4 backend tests and 2 frontend tests. I kept the number small on
purpose and made each one cover a real risk rather than repeating the code.

The four backend tests are:

1. Only the four allowed status changes work, nothing can move once closed,
   and a ticket cannot skip ahead.
2. The due date matches the priority, and changing the priority recalculates
   from the original creation date rather than from today.
3. Work cannot start without an active agent, including the case where the
   agent was active when assigned and was deactivated afterwards.
4. The system sets the resolved and closed dates itself, reopening clears the
   resolved date, and a closed ticket refuses every kind of change.

The two frontend tests check that searching and filtering happen on the server
rather than in the browser, and that typing in the search box waits until you
stop typing instead of sending a request per letter.

---

## What I would do with more time

- Add a login, so the person acting on a ticket is known instead of typed in.
- Add tests that go through the real endpoints, not only the rules.
- Add a Docker file for the database so setup does not depend on the port being free.
- Handle two agents changing the same ticket at the same moment.
- Improve the look of the frontend. I kept it plain on purpose so I could spend
  the time on the rules, which is what the task said it was grading.

---

## How long it took

Around 24 hours.
