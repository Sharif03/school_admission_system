## 1. Technical Decisions

### 1.1 Why Enum for Hold Status?

Used `HoldStatus` enum to manage application states:

```csharp
public enum HoldStatus
{
    Held = 1,
    Confirmed = 2,
    Expired = 3,
    Cancelled = 4
}
```

It provides type safety, prevents invalid status values, and makes status transitions easier to maintain. Values are stored as integers in PostgreSQL.

### 1.2 Why Guid?

Used `Guid` as primary keys for classes and holds because it provides globally unique identifiers, avoids sequential ID dependencies, and reduces predictable IDs.

### 1.3 Why Clean Architecture?

Separated the backend into four layers:

- **Domain:** Entities, enums, and business rules.
- **Application:** DTOs, interfaces, and services.
- **Infrastructure:** EF Core, PostgreSQL, and repositories.
- **API:** Controllers, middleware, and configuration.

This separation improves maintainability, testability, and independence from infrastructure.

### 1.4 How Is Race Condition Handled?

To ensure `Held Seats + Confirmed Seats` never exceeds `Total Seats`, I implemented PostgreSQL transactions with row-level locking using `SELECT ... FOR UPDATE`.

The reservation process:

1. Begin a transaction.
2. Lock the corresponding class row.
3. Expire overdue holds.
4. Count active HELD and CONFIRMED applications.
5. Check seat availability.
6. Create the hold if capacity is available.
7. Commit the transaction.

Concurrent requests for the same class wait for the lock to be released. This protects capacity even when multiple backend processes share the same PostgreSQL database.

I also added a filtered unique index:

```sql
CREATE UNIQUE INDEX ...
ON seat_holds ("ClassId", "StudentId")
WHERE "Status" IN (1, 2);
```

This prevents a student from having multiple active or confirmed applications for the same class.

### 1.5 Seat Lock, Expiration, and Cancellation

- **HELD:** Temporarily reserves one seat.
- **CONFIRMED:** Permanently occupies one seat.
- **EXPIRED:** Releases the reserved seat after the hold expires.
- **CANCELLED:** Releases the reserved seat after cancellation.

The hold duration is configurable through `HOLD_DURATION_SECONDS` (default: 600 seconds).

Confirmation is idempotent, so repeated requests do not increase the confirmed seat count.

```text
Available Seats = Total Seats - (Held Seats + Confirmed Seats)
```

Only non-expired HELD and CONFIRMED applications count as occupied seats.

**Note:** The current implementation processes expiration during relevant backend operations. A periodic background worker for expiration without API activity is still pending.

### 1.6 Why Records for DTOs?

Used C# records for DTOs because they are concise, data-focused, support value-based equality, and reduce boilerplate code.

```csharp
public record CreateClassRequest(string Name, int TotalSeats);
```

DTOs carry data and do not require domain entity business behavior.


### 1.7 Why .dockerignore?

Added `.dockerignore` files to exclude unnecessary files from Docker build contexts, reduce image build time, and avoid copying local dependencies and development artifacts.

Examples:

```text
bin/
obj/
node_modules/
dist/
.git/
.env.local
```

## 2. Data Persistence

PostgreSQL is the primary database. EF Core migrations manage schema changes, while a persistent Docker volume preserves data across container restarts.

## 3. Error Handling

Centralized exception middleware returns appropriate HTTP responses:

- `400`: Invalid input.
- `404`: Resource not found.
- `409`: Conflict or unavailable seats.
- `500`: Unexpected server error.

## 4. Scope and Limitations

- Students are identified through the `X-Student-Id` header; full authentication is not implemented.
- Email/SMS notifications, payment processing, and admission lottery are outside the assignment scope.
- A standalone background expiry worker is pending implementation.