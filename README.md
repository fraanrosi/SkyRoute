# SkyRoute — Flight Search & Booking

A working slice of a flight aggregator platform. Search across two mocked airline providers, sort the results, and book a flight whose document-type requirement (Passport vs. National ID) is determined by whether the route is international or domestic.

Stack: **.NET 8 Web API** (backend) + **Angular 17 standalone components** (frontend). Built per the architectural blueprint in [`CLAUDE.md`](./CLAUDE.md).

---

## Repo layout

```
SkyRoute/
├── backend/
│   ├── SkyRoute.sln
│   ├── SkyRoute.API/           # Web API (controllers, services, providers, pricing)
│   └── SkyRoute.Tests/         # xUnit + Moq tests
├── frontend/
│   └── skyroute-web/           # Angular 17 workspace, standalone components
├── plans/                      # Implementation plans (backend + frontend)
├── CLAUDE.md                   # Architectural blueprint
└── README.md
```

---

## Prerequisites

- .NET 8 SDK (`dotnet --version` should report `8.x`).
- Node 18+ and npm 9+ (Angular 17 requirements).

## Run the stack

Open two terminals.

**Terminal 1 — backend (port 5180 HTTP, 7183 HTTPS):**

```powershell
cd backend
dotnet run --project SkyRoute.API --launch-profile http
```

Swagger UI: <http://localhost:5180/swagger>

**Terminal 2 — frontend (port 4200, proxies `/api` to 5180):**

```powershell
cd frontend/skyroute-web
npm start
```

App: <http://localhost:4200>

## Run the tests

```powershell
cd backend
dotnet test
```

21 tests cover the two pricing strategies (rounding, floor enforcement) and the aggregator's failure-isolation + input validation.

---

## What's implemented

### Backend
- **Provider Pattern** — each airline implements `IFlightProvider`; the aggregator receives them via DI. Adding a third provider is one new class + one DI line.
- **Strategy Pattern for pricing** — `GlobalAirPricingStrategy` (+15% fuel surcharge) and `BudgetWingsPricingStrategy` (-10%, $29.99 floor) are injected into their providers.
- **`FlightAggregatorService`** runs all providers in parallel via `Task.WhenAll`; per-provider exceptions are caught, logged, and isolated so siblings still return.
- **`BookingService`** re-validates document type against the route on every booking (international ⇒ Passport, domestic ⇒ NationalId).
- **In-memory storage** for flights (populated during search so booking can look up) and bookings.
- **Cancellation tokens** flow through controllers → services → providers.
- **Swagger UI**, **CORS for the Angular dev server**, **structured logging** via `ILogger<T>`.

### Frontend
- **Standalone components**, no NgModules.
- **Reactive Forms** + **FormArray** for the dynamic passenger list (one form group per passenger).
- **Reactive document-type** — label and validator on each passenger's document field swap between "Passport Number" (regex `^[A-Z0-9]{6,9}$`) and "National ID" (`^\d{7,8}$`) based on the selected flight's route.
- **Frontend-only sorting** — price asc/desc, duration, departure time — operates on cached results, no extra API call.
- **Explicit loading / empty / error states** on search and booking.
- **Lazy-loaded routes** via `loadComponent` for each page.
- **`proxy.conf.json`** maps `/api` to the backend so the dev server stays origin-clean.

---

## Documented trade-offs

- **No real database.** Flights and bookings live in `ConcurrentDictionary` singletons; everything is lost on restart. Production would use SQL Server or PostgreSQL behind EF Core.
- **No authentication.** Out of scope for the challenge. Production would add JWT auth on the API and an interceptor on the Angular side.
- **Mocked providers.** Flight generation is deterministic by route + date so the same query returns the same results. Production would replace the body of `GlobalAirProvider.SearchAsync` / `BudgetWingsProvider.SearchAsync` with real HTTP integrations — the `IFlightProvider` contract stays the same.
- **No cache.** Every search hits both providers in parallel. Production would add a short-TTL Redis cache keyed by request.
- **No resilience patterns yet.** Production would wrap provider calls with Polly (timeout + retry + circuit breaker).

## What I'd add with more time

- E2E tests with Playwright (search → sort → book → confirmation happy path).
- Integration tests for the booking flow (NationalId on international ⇒ 400, etc.).
- Polly resilience layer around provider calls.
- Internationalization of the UI (currency, dates, labels).
- A small UI polish pass for the booking confirmation page.

---

See [`plans/`](./plans/) for the per-stack implementation plans.
