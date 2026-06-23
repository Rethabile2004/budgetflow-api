# BudgetFlow API

A personal finance tracking REST API built with ASP.NET Core. Users register, log in, and manage their own categories, transactions, and budgets — with monthly spending reports generated automatically in the background.

**Live API:** https://budgetflow-api-dw3i.onrender.com
**Swagger docs:** https://budgetflow-api-dw3i.onrender.com/swagger
**Hangfire dashboard:** https://budgetflow-api-dw3i.onrender.com/hangfire

> Note: hosted on Render's free tier, so the first request after a period of inactivity may take a few seconds while the service spins back up.

---

## Overview

BudgetFlow lets a user:

- Register and log in securely (JWT access tokens + rotating refresh tokens)
- Create custom categories, marked as Income or Expense
- Log transactions against those categories
- Set a monthly spending limit (budget) per category, with live tracking of amount spent vs. remaining
- View a summary of income, expenses, and balance for any date range, broken down by category
- Automatically receive a generated monthly report on the 1st of every month, viewable via the API

All data is scoped to the logged-in user — no user can see or modify another user's data.

---

## Tech Stack

- **ASP.NET Core 8** — Web API
- **Entity Framework Core** — ORM
- **PostgreSQL** — database (production), via Npgsql provider
- **ASP.NET Core Identity** — user management and password hashing
- **JWT Bearer Authentication** — stateless auth with refresh token rotation
- **Hangfire** — scheduled background job processing (monthly report generation)
- **Swagger / OpenAPI** — interactive API documentation
- **Docker** — containerized for deployment
- **Render** — hosting (Web Service + PostgreSQL)

---

## Architecture

- **Controllers** — thin, handle HTTP concerns only
- **Services** — business logic, one per domain area (Auth, Categories, Transactions, Budgets, Summary, Reports)
- **DTOs** — request/response shapes, kept separate from EF Core models
- **Global exception handling middleware** — consistent error response shape across the whole API, using custom exception types (`NotFoundException`, `BadRequestException`, `UnauthorizedException`)
- **User-scoped queries** — every query filters by the authenticated user's ID, extracted from JWT claims via `IHttpContextAccessor`

---

## Authentication

This API uses short-lived JWT access tokens paired with longer-lived, rotating refresh tokens.

| Token | Lifetime | Purpose |
|---|---|---|
| Access token | 60 minutes | Sent with every authenticated request |
| Refresh token | 7 days | Exchanged for a new access token once it expires |

Flow:
1. Register or log in → receive both tokens
2. Send the access token on every request: `Authorization: Bearer <accessToken>`
3. When the access token expires (`401 Unauthorized`), call `/api/auth/refresh` with the refresh token to get a new pair
4. Refresh tokens rotate — each use invalidates the old one and issues a new one

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- PostgreSQL (local instance or via Docker)
- Docker (optional, for containerized run)

### Setup

```bash
git clone https://github.com/Rethabile2004/budgetflow-api.git
cd budgetflow-api
```

Update `appsettings.json` with your local PostgreSQL connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=BudgetFlowDb;Username=postgres;Password=yourpassword"
}
```

Apply migrations:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

Swagger UI will be available at `https://localhost:{port}/swagger`.

### Running with Docker

```bash
docker build -t budgetflow-api .
docker run -p 8080:8080 budgetflow-api
```

---

## API Endpoints

### Auth — `/api/auth`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/register` | No | Create a new account |
| POST | `/login` | No | Log in |
| POST | `/refresh` | No | Exchange a refresh token for a new access token |
| POST | `/revoke` | Yes | Log out — invalidate a refresh token |

### Categories — `/api/categories`

| Method | Route | Description |
|---|---|---|
| GET | `/` | List all categories for the logged-in user |
| GET | `/{id}` | Get a single category |
| POST | `/` | Create a category |
| PUT | `/{id}` | Update a category |
| DELETE | `/{id}` | Delete a category (blocked if transactions are linked to it) |

### Transactions — `/api/transactions`

| Method | Route | Description |
|---|---|---|
| GET | `/` | List transactions — supports filtering by `from`, `to`, `categoryId`, `type`, plus `page`/`pageSize` |
| GET | `/{id}` | Get a single transaction |
| POST | `/` | Create a transaction |
| PUT | `/{id}` | Update a transaction |
| DELETE | `/{id}` | Delete a transaction |

### Budgets — `/api/budgets`

| Method | Route | Description |
|---|---|---|
| GET | `/?month={m}&year={y}` | List budgets for a given month/year, with live spent/remaining amounts |
| GET | `/{id}` | Get a single budget |
| POST | `/` | Create a budget |
| PUT | `/{id}` | Update a budget's limit |
| DELETE | `/{id}` | Delete a budget |

### Summary — `/api/summary`

| Method | Route | Description |
|---|---|---|
| GET | `/?from={date}&to={date}` | Income/expense totals and balance for a date range, broken down by category |

### Reports — `/api/reports`

Read-only — reports are generated automatically by a Hangfire background job on the 1st of every month.

| Method | Route | Description |
|---|---|---|
| GET | `/` | List all past monthly reports |
| GET | `/{id}` | Get a single report |

Full request/response examples are available via the [Swagger UI](https://budgetflow-api-dw3i.onrender.com/swagger).

---

## Error Handling

All errors follow a consistent shape:

```json
{
  "statusCode": 404,
  "message": "Category not found."
}
```

| Status | Meaning |
|---|---|
| 400 | Validation error, duplicate data, or invalid input |
| 401 | Missing, expired, or invalid token |
| 404 | Resource not found, or doesn't belong to the authenticated user |
| 500 | Unexpected server error |

---

## Background Jobs

A recurring Hangfire job runs at midnight on the 1st of every month, generating a spending report for every user based on their previous month's transactions. Job status and history can be viewed on the [Hangfire dashboard](https://budgetflow-api-dw3i.onrender.com/hangfire).

---

## Project Status

Actively maintained as part of an ongoing backend development learning path. Built after [TaskFlow API](https://github.com/Rethabile2004) and Library API, with a focus on going deeper into authentication, multi-user data isolation, and scheduled background processing.
