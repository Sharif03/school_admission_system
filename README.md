# School Admission System

A full-stack school admission management system built with **ASP.NET Core, Entity Framework Core, PostgreSQL, React, and Docker**.

The application allows students to reserve admission seats temporarily, confirm reservations before expiration, and track their applications.

## Features

- Create admission classes and configure seat capacity.
- View total, held, confirmed, and available seats.
- Temporarily reserve and confirm admission seats.
- Cancel active reservations.
- Track personal admission applications.
- Prevent duplicate active or confirmed applications for the same class.
- Prevent overbooking using PostgreSQL transactions and row-level locking.
- Automatically refresh seat availability in the frontend.
- Persist application data using PostgreSQL.
- Run the application using Docker Compose.

## Technology Stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10) |
| Language | C# |
| Architecture | Clean Architecture |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Frontend | React, Vite, CSS |
| Testing | xUnit, Vitest, React Testing Library |
| Containerization | Docker, Docker Compose |

## Project Structure

```text
SchoolAdmissionSystem/
├── backend/
│   ├── SchoolAdmission.Domain/
│   ├── SchoolAdmission.Application/
│   ├── SchoolAdmission.Infrastructure/
│   ├── SchoolAdmission.API/
│   └── SchoolAdmission.Tests/
│
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── services/
│   │   ├── App.jsx
│   │   └── App.test.jsx
│   ├── Dockerfile
│   └── package.json
│
├── docker-compose.yml
```

### Backend Architecture

- **Domain:** Core entities, enums, and business rules.
- **Application:** DTOs, interfaces, and application services.
- **Infrastructure:** EF Core, PostgreSQL, migrations, and repositories.
- **API:** Controllers, middleware, and application configuration.

## Application Workflow

1. A student enters their 17-digit Student ID.
2. The student selects an admission class.
3. The backend checks available capacity.
4. A temporary `HELD` reservation is created.
5. The student confirms the reservation before expiration.
6. Expired or cancelled reservations release the seat.

**Seat availability:**

```text
Available Seats = Total Seats - (Held Seats + Confirmed Seats)
```

The hold duration is configurable:

```env
HOLD_DURATION_SECONDS=600
```

Default duration: 600 seconds (10 minutes).

## Concurrency Handling

The backend uses PostgreSQL transactions and row-level locking (`SELECT ... FOR UPDATE`) when creating reservations.

This serializes competing reservation requests for the same class and prevents the total occupied seats from exceeding capacity.

A filtered unique index also prevents duplicate active or confirmed applications for the same student and class.

## Run with Docker Compose

### Prerequisites

- Docker Desktop
- Docker Compose

### Start the application

```bash
docker compose up --build
```

### Application URLs

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:8080 |

### Stop the application

```bash
docker compose down
```

PostgreSQL data is preserved in the configured Docker volume. Avoid `docker compose down -v` if you want to retain database data.

## Run Locally

### Prerequisites

- .NET SDK 10
- Node.js 22+
- PostgreSQL

### Backend

Configure the PostgreSQL connection string, then run:

```bash
dotnet run --project backend/SchoolAdmission.API
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:5173.

Configure the backend URL in `frontend/.env`:

```env
VITE_API_BASE_URL=http://localhost:8080
```

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/classes` | List admission classes |
| POST | `/classes` | Create an admission class |
| GET | `/classes/{classId}` | Get class details |
| POST | `/classes/{classId}/holds` | Reserve a seat |
| GET | `/holds/{holdId}` | Get reservation details |
| POST | `/holds/{holdId}/confirm` | Confirm reservation |
| DELETE | `/holds/{holdId}` | Cancel reservation |
| GET | `/me/holds` | View student applications |

Student-specific endpoints require the following header:

```http
X-Student-Id: 12345678901234567
```

The Student ID must contain exactly 17 digits.

## Run Tests

### Backend

```bash
dotnet build SchoolAdmissionSystem.sln
dotnet test SchoolAdmissionSystem.sln
```

### Frontend

```bash
cd frontend
npm run test:run
npm run build
npm run lint
```

The backend tests cover application logic and concurrent seat reservations using PostgreSQL. Frontend tests cover components and admission workflows.

## Documentation

- [ASSUMPTIONS.md](ASSUMPTIONS.md) — Technical decisions, assumptions, and implementation details.
- [AI_USAGE.md](AI_USAGE.md) — AI tools and development assistance.