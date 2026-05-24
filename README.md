# SkyRoute — Flight Search & Booking

Flight aggregator slice: search across two mocked airline providers, sort the results, and book a flight whose document field (Passport vs. National ID) switches based on the route being international or domestic.

**Stack:** .NET 8 Web API + Angular 17 standalone components.

---

## Run

Requires .NET 8 SDK and Node 18+.

**Backend** — `http://localhost:5180` (Swagger at `/swagger`):

```powershell
cd backend
dotnet run --project SkyRoute.API --launch-profile http
```

**Frontend** — `http://localhost:4200` (proxies `/api` to the backend):

```powershell
cd frontend/skyroute-web
npm install   # first time only
npm start
```

## Tests

```powershell
cd backend && dotnet test                                   # 42 xUnit tests
cd frontend/skyroute-web && npx ng test --watch=false --browsers=ChromeHeadless   # 2 Karma tests
```

---

## Architecture

- **Provider Pattern** — each airline implements `IFlightProvider`; the aggregator gets them via DI. New provider = one new class, zero changes elsewhere.
- **Strategy Pattern** — pricing rules (GlobalAir +15% fuel, BudgetWings −10% with $29.99 floor) live in `IPricingStrategy` implementations, injected per provider.
- **`FlightAggregatorService`** runs all providers in parallel via `Task.WhenAll` and isolates per-provider failures so siblings still return.
- **`BookingService`** re-validates document type vs. route on every booking (international ⇒ Passport, domestic ⇒ NationalId).
- **In-memory storage** (`ConcurrentDictionary`) for flights and bookings; cancellation tokens end-to-end; structured logging via `ILogger<T>`.
- **Angular** — standalone components, lazy-loaded routes, Reactive Forms + `FormArray` for the dynamic passenger list, frontend-only sorting via signals.

## Trade-offs

- **No database** — `ConcurrentDictionary` singletons; data is lost on restart. Production: SQL Server / PostgreSQL via EF Core.
- **No auth** — `GET /api/bookings` (and the `/bookings` page that uses it) returns *every* booking in the demo store. Production: JWT + admin-only on that endpoint.
- **Mocked providers** — deterministic mock data by route + date. Production: real HTTP integrations behind the same `IFlightProvider` contract.
- **No cache, no resilience layer** — every search hits both providers; no Polly timeout/retry/circuit-breaker yet.

## What I'd add with more time

E2E tests (Playwright), Polly around provider calls, real DB migration, i18n on the UI.
