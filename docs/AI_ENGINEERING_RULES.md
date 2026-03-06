# AI_ENGINEERING_RULES.md

## Purpose

This repository implements a **.NET Clean Architecture technical assessment**.

AI assistants working on this project must prioritize:

* **Clarity**
* **Correctness**
* **Architectural discipline**
* **Maintainability**

The implementation should reflect **senior-level engineering quality within a time-boxed exercise**.

Avoid unnecessary complexity.
Favor **readable, production-quality code**.

---

# Core Engineering Principles

Follow these principles strictly.

## KISS — Keep It Simple

Prefer simple implementations that are easy to understand and maintain.

## DRY — Don’t Repeat Yourself

Avoid duplication when it improves clarity and maintainability.

## YAGNI — You Aren’t Gonna Need It

Do not introduce abstractions, frameworks, or features that are not required.

## SOLID

Design code with clear responsibilities and strong separation of concerns.

---

## Additional Engineering Guidelines

* Prefer **clarity over cleverness**
* Avoid speculative abstractions
* Keep classes **small and focused**
* Use **explicit naming**
* Avoid **magic strings and magic numbers**
* Keep methods **short and readable**
* Do not introduce frameworks unless clearly required

---

# Clean Architecture Rules

This project follows **Clean Architecture**.

## Allowed Dependency Direction

```
API → Application → Domain
Infrastructure → Application → Domain
```

## Forbidden Dependencies

The following dependencies must **never occur**:

```
Domain → Application
Domain → Infrastructure
Application → Infrastructure
```

---

## Layer Responsibilities

### Domain

Contains **core business entities and rules**.

Rules:

* Must not reference frameworks, databases, or UI
* Must not depend on Application or Infrastructure
* Must contain only **business concepts**

---

### Application

Contains **use cases and orchestration logic**.

Responsibilities:

* Application services
* DTOs
* Repository interfaces
* Business exceptions
* Error codes

Rules:

* Must not reference Infrastructure
* Must not expose domain entities to external layers

---

### Infrastructure

Implements **technical concerns**.

Responsibilities:

* Repository implementations
* EF Core persistence
* Database access

Rules:

* May depend on Application and Domain
* Must not contain business rules

---

### API

Handles **HTTP transport**.

Responsibilities:

* Accept HTTP requests
* Call Application services
* Return HTTP responses

Rules:

* Controllers must remain **thin**
* Controllers must not contain business logic

---

# Domain Layer Rules

Domain represents **core business concepts**.

## Entity Design

Entities must:

* Enforce valid state through constructors
* Prefer **private setters**
* Avoid framework dependencies
* Avoid persistence attributes
* Contain business invariants where appropriate

---

## Domain Model

### Patient (Aggregate Root)

```
Id
FirstName
LastName
Contacts
```

### Appointment

```
Id
PatientId
Title
StartTime
EndTime
Notes
```

---

## Value Objects

`Contact` should be implemented as a **Value Object**.

Example:

```
Contact
  - Type
  - Value
```

---

## Possible Domain Invariants

Examples:

* Appointment start time must be before end time
* Appointment time ranges cannot overlap

---

# Application Layer Rules

The Application layer orchestrates business logic.

It contains:

* Application services
* DTOs
* Repository interfaces
* Business exceptions
* Error codes

Rules:

* Application must not reference Infrastructure
* DTOs must be used for API communication
* Domain entities must not be returned directly
* Repository interfaces must be defined here

Example business rule handled in Application:

```
Prevent overlapping appointments
```

---

# Error Handling Rules

Business errors must use **error codes**.

Example error codes:

```
AppointmentOverlap
AppointmentNotFound
PatientNotFound
ValidationError
UnknownError
```

Business errors must use:

```
BusinessException(ErrorCode errorCode, string message)
```

---

## API Error Response Format

Example:

```
{
  "errorCode": "AppointmentOverlap",
  "message": "Appointment overlaps with an existing appointment"
}
```

Avoid complex error frameworks.
Keep the response structure consistent.

---

# Infrastructure Layer Rules

Infrastructure implements technical persistence.

Responsibilities:

* Repositories
* EF Core DbContext
* Database access

Rules:

* Infrastructure depends on Application and Domain
* Infrastructure must not contain business rules
* Persistence logic belongs here

`DbContext` acts as the **Unit of Work**.

Do not introduce a separate UnitOfWork abstraction unless explicitly required.

---

# API Layer Rules

Controllers must remain **thin**.

Controllers should:

* Accept HTTP requests
* Call Application services
* Return HTTP responses

Controllers must **not contain business logic**.

---

## HTTP Status Codes

```
GET    → 200
POST   → 201
PUT    → 204
DELETE → 204
```

---

## API Endpoints

```
GET    /api/appointments?date=YYYY-MM-DD
GET    /api/appointments/{id}
POST   /api/appointments
PUT    /api/appointments/{id}
DELETE /api/appointments/{id}
```

---

## Optional Endpoint

CSV export:

```
GET /api/appointments/export
```

CSV fields:

```
Id
Title
PatientName
StartTime
```

---

# Async Programming Rules

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

Prefer:

```
Task
CancellationToken
```

Database calls must use **EF Core async methods**.

---

# Dependency Injection Rules

Use `Microsoft.Extensions.DependencyInjection`.

Rules:

* Prefer **constructor injection**
* Avoid service locator patterns
* Register dependencies in `Program.cs`
* Repositories must be registered as **Scoped**

---

# Repository Rules

Use **specific repositories**.

Examples:

```
IAppointmentRepository
IPatientRepository
```

Rules:

* Return `IReadOnlyList` for collections
* Do not expose `IQueryable` outside Infrastructure
* Use projection queries for search operations

Example projection:

```
Select → AppointmentSummaryDto
```

---

# DTO Design Rules

Use separate DTOs for:

* Search/List results
* Detailed entity views

Examples:

```
AppointmentSummaryDto
AppointmentDetailsDto
```

Search DTOs should remain **lightweight**.

Example fields:

```
Id
Title
PatientName
StartTime
```

Full details should only be returned when retrieving a specific record.

---

# Validation Rules

Validation must occur in:

* Domain invariants
* Application services

Controllers must **not perform business validation**.

Example validation:

```
Prevent overlapping appointments
```

---

# WPF MVVM Rules

The WPF client must follow **MVVM**.

## Views

* Contain only UI bindings
* No business logic

## ViewModels

* Hold UI state
* Execute commands
* Call API services

API communication should occur through:

* ViewModels
* Dedicated API client services

---

# Code Style Rules

* One class per file
* PascalCase for types
* camelCase for variables
* Remove unused using statements
* Namespaces must match folders
* Prefer readability over compact code

---

# AI Safety Rules

AI assistants must **respect the existing architecture**.

Do not introduce:

* Mediator pattern
* Specification pattern
* Complex CQRS frameworks
* Event sourcing
* Authentication systems

unless explicitly requested.

Do not restructure the architecture unless asked.

Avoid unnecessary design patterns.

---

# Assessment Strategy

This is a **time-boxed technical assessment**.

Priorities:

1. Clear architecture
2. Working functionality
3. Clean, readable code

Working software is more important than theoretical perfection.

Avoid unnecessary abstractions.
