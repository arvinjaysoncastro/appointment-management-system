# AI_GUARDRAILS.md

## Purpose

This file defines **non-negotiable guardrails** for AI-assisted coding in this repository.

AI assistants (Copilot, Cursor, ChatGPT) must treat these rules as **hard constraints**.

If a request conflicts with these guardrails, the assistant must propose a **compliant alternative** rather than violating the architecture.

This repository is a **time-boxed .NET Clean Architecture technical assessment**, therefore:

* Prioritize **clarity and correctness**
* Avoid **architecture drift**
* Avoid **speculative patterns or over-engineering**
* Prefer **simple, maintainable solutions**

---

# 1. Architecture Guardrails

## Allowed dependency direction

Only the following dependency flow is allowed:

```
API → Application → Domain
Infrastructure → Application → Domain
```

## Forbidden dependencies

The following dependencies are **never allowed**:

* Domain → Application
* Domain → Infrastructure
* Application → Infrastructure
* WPF Client → Infrastructure / DbContext

The WPF client must communicate **only with the API**.

---

## Forbidden shortcuts

These shortcuts must **never occur**:

* Controllers calling repositories directly
* Controllers calling DbContext directly
* ViewModels accessing DbContext directly
* Returning Domain entities from API endpoints
* Adding EF Core attributes to Domain entities

If a solution requires one of these, redesign it using **Application services and interfaces**.

---

# 2. Thin Controller Rules

Controllers are **transport adapters only**.

Controllers may contain only:

* HTTP request binding
* Calling an Application service
* Returning HTTP responses

Controllers must **never contain**:

* business rules
* scheduling logic
* overlap validation
* repository calls
* DbContext access
* entity mapping

All business logic belongs in **Application services or Domain invariants**.

---

# 3. DTO Rules

API endpoints must return **DTOs only**.

Never expose:

```
Domain.Entities.*
```

DTO guidelines:

* **Search/List endpoints** return *summary DTOs*
* **Detail endpoints** return *detail DTOs*

Example DTOs:

```
AppointmentSummaryDto
AppointmentDetailsDto
```

---

# 4. Repository Rules

Repository interfaces must live in the **Application layer**.

Repository implementations must live in **Infrastructure**.

Rules:

* Return `IReadOnlyList` for collections
* Never expose `IQueryable` outside Infrastructure
* Prefer projection queries for list/search endpoints

Example projection:

```
Select → AppointmentSummaryDto
```

Repositories must **not contain business logic**.

---

# 5. EF Core / SQLite Rules

EF Core is allowed **only in Infrastructure**.

Domain and Application must **not reference EF Core**.

The `DbContext` acts as the **Unit of Work**.

Do not introduce a separate `UnitOfWork` abstraction unless explicitly required.

If required, it must be a thin wrapper around:

```
SaveChangesAsync()
```

---

# 6. Error Handling Rules

Business errors must use:

```
BusinessException(errorCode, message)
```

Error codes must be defined in the **Application layer**.

API must translate business exceptions to:

```
400 BadRequest
{
  "errorCode": "...",
  "message": "..."
}
```

Unexpected errors must return:

```
500
{
  "errorCode": "UNKNOWN_ERROR",
  "message": "An unexpected error occurred."
}
```

Avoid complex error frameworks.

Keep the response format stable.

---

# 7. Async Rules

All IO operations must be **asynchronous**.

Always use:

```
async / await
```

Never use:

```
.Result
.Wait()
```

Always propagate:

```
CancellationToken
```

---

# 8. Naming Rules

Use clear and explicit naming.

Examples:

```
IAppointmentRepository
AppointmentSummaryDto
AppointmentDetailsDto
```

Avoid:

* single-letter variables
* unclear abbreviations
* misleading class names

---

# 9. Scope Guardrails (Timebox)

Default project scope includes only:

* Appointments CRUD API
* SQLite persistence
* WPF MVVM client
* MainWindow + at least one UserControl
* HttpClient API communication via IoC service

Optional features (only if time allows):

* CSV export
* Patient search
* UI validation
* Overlap validation

Do **not introduce**:

* authentication
* authorization
* roles/claims
* multi-tenancy
* mediator frameworks
* CQRS infrastructure
* event sourcing

unless explicitly requested.

---

# 10. Stop-and-Ask Conditions

If a change would require:

* violating dependency direction
* adding architectural layers
* introducing complex patterns (Mediator, CQRS, Event Sourcing)
* adding authentication flows

Stop and propose a **compliant alternative**.

---

# 11. Recommended AI Prompt Pattern

When using Copilot or Cursor, prompts should include:

```
Follow AI_ENGINEERING_RULES.md and AI_GUARDRAILS.md.
Do not refactor unrelated code.
Make the smallest change necessary.
Keep controllers thin and business logic in Application services.
```

Example prompt:

> Follow AI_ENGINEERING_RULES.md and AI_GUARDRAILS.md.
> Add WPF HttpClient API client + ViewModel for loading appointments.
> Do not change API or Infrastructure.

---

# 12. Definition of Done

A change is acceptable only if:

* The solution builds successfully
* No forbidden dependencies are introduced
* Controllers remain thin
* Business logic remains in Application/Domain
* API returns consistent error responses
* Async and cancellation patterns are respected
