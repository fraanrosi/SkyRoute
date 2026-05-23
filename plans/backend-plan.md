# SkyRoute — Backend Plan (.NET 8 Web API)

> Companion to [frontend-plan.md](./frontend-plan.md).

## Context

This plan scaffolds the .NET 8 Web API that powers the **Flight Search & Booking** vertical slice: parallel aggregation of two mocked providers (GlobalAir +15% fuel, BudgetWings −10% with $29.99 floor), in-memory persistence, and booking with document-type validation tied to domestic vs. international routing.

**Outcome:** a runnable .NET 8 Web API serving `/api/airports`, `/api/flights/search`, `/api/bookings`, and `/api/bookings/{reference}`, with xUnit tests for pricing and aggregator failure-isolation green.

**Repo layout (backend slice):**

```
SkyRoute/
└── backend/
    ├── SkyRoute.sln
    ├── SkyRoute.API/
    └── SkyRoute.Tests/
```

---

## A1. Solution & projects
- Create `backend/SkyRoute.sln`.
- Create `backend/SkyRoute.API/` (`dotnet new webapi -f net8.0 --use-program-main`).
- Create `backend/SkyRoute.Tests/` (`dotnet new xunit -f net8.0`) referencing `SkyRoute.API`.
- Add both to the solution.

## A2. Domain layer — DTOs, entities, enums
Folder: `SkyRoute.API/Models/`

- `Dtos/FlightSearchRequest.cs`, `FlightResult.cs`, `BookingRequest.cs`, `PassengerDto.cs`, `BookingResponse.cs`, `Airport.cs` — DTOs returned by the API.
- `Entities/Flight.cs`, `Entities/Booking.cs` — internal storage shapes (Flight holds the **base fare** before pricing rules; Booking holds the snapshot of the priced result + passenger list + status).
- `Enums/CabinClass.cs`, `DocumentType.cs`, `BookingStatus.cs`.

## A3. Provider Pattern — `Providers/`
- `IFlightProvider.cs` — `Task<IEnumerable<FlightResult>> SearchAsync(FlightSearchRequest, CancellationToken)` + `string Name { get; }`.
- `GlobalAirProvider.cs` — generates 2–3 deterministic mock flights per route, applies `GlobalAirPricingStrategy`.
- `BudgetWingsProvider.cs` — same shape, applies `BudgetWingsPricingStrategy`.
- Mock flight generation is deterministic-but-realistic (seeded by route + date hash so the same query returns the same flights, simplifying manual testing).

## A4. Pricing strategies — `Pricing/`
- `IPricingStrategy.cs` — `decimal Apply(decimal baseFare)`.
- `GlobalAirPricingStrategy.cs` — `Math.Round(baseFare * 1.15m, 2)`.
- `BudgetWingsPricingStrategy.cs` — `Math.Max(Math.Round(baseFare * 0.90m, 2), 29.99m)`.
- Each provider receives its strategy via constructor injection so swap-in is trivial.

## A5. Services — `Services/`
- `FlightAggregatorService.cs` — receives `IEnumerable<IFlightProvider>` via DI; runs all providers in parallel with `Task.WhenAll`; wraps each call in a `SafeSearchAsync` helper that catches per-provider exceptions, logs via `ILogger<FlightAggregatorService>`, and returns an empty list for the failed provider so siblings still surface. Computes `TotalPrice = PricePerPassenger * NumberOfPassengers` if not already set by the provider.
- `BookingService.cs` — accepts `BookingRequest`, looks up the flight via the in-memory store, **re-validates** route type vs. `DocumentType` (international ⇒ all passengers must be Passport; domestic ⇒ all NationalId; otherwise reject with a clear error), generates a booking reference (`SKY-{yyyy}-{6-char upper hex}`), persists to `InMemoryBookingStore`, returns `BookingResponse`.

## A6. Data layer — `Data/`
- `AirportSeed.cs` — the 7-airport static list (EZE, AEP, COR, MIA, JFK, GRU, SCL).
- `InMemoryFlightStore.cs` — `ConcurrentDictionary<string, Flight>` keyed by `FlightId`; populated when providers return results so the booking endpoint can look them up.
- `InMemoryBookingStore.cs` — `ConcurrentDictionary<string, Booking>` keyed by booking reference.

## A7. Controllers — `Controllers/`
- `AirportsController.cs` → `GET /api/airports` returns `AirportSeed.Airports`.
- `FlightsController.cs` → `POST /api/flights/search` calls `FlightAggregatorService`, returns `FlightResult[]`. Validates `NumberOfPassengers` 1–9, future date, and known origin/destination codes.
- `BookingsController.cs` → `POST /api/bookings` + `GET /api/bookings/{reference}` (optional extra, included per decision).
- All endpoints accept `CancellationToken`.

## A8. Cross-cutting — `Program.cs`
- DI registration: both providers as `IFlightProvider`, both strategies, the aggregator, the booking service, the two stores as singletons.
- **CORS:** named policy `AllowAngularDev` allowing `http://localhost:4200` with any header/method; applied in dev only.
- **Swagger:** Swashbuckle enabled in dev (browseable at `/swagger`).
- **Structured logging:** default `ILogger<T>` injected throughout.

## A9. Tests — `SkyRoute.Tests/`
- `Pricing/GlobalAirPricingStrategyTests.cs` — rounding cases incl. half-up edge.
- `Pricing/BudgetWingsPricingStrategyTests.cs` — discount, floor enforced at $29.99, rounding.
- `Services/FlightAggregatorServiceTests.cs` — one provider throws, the other returns results; aggregator yields the successful provider's results and logs the failure.

## A10. Critical files
- [backend/SkyRoute.API/Program.cs](../backend/SkyRoute.API/Program.cs)
- [backend/SkyRoute.API/Providers/IFlightProvider.cs](../backend/SkyRoute.API/Providers/IFlightProvider.cs)
- [backend/SkyRoute.API/Pricing/IPricingStrategy.cs](../backend/SkyRoute.API/Pricing/IPricingStrategy.cs)
- [backend/SkyRoute.API/Services/FlightAggregatorService.cs](../backend/SkyRoute.API/Services/FlightAggregatorService.cs)
- [backend/SkyRoute.API/Services/BookingService.cs](../backend/SkyRoute.API/Services/BookingService.cs)
- [backend/SkyRoute.API/Controllers/BookingsController.cs](../backend/SkyRoute.API/Controllers/BookingsController.cs)

---

## Verification

- `dotnet build` — clean.
- `dotnet test` — all pricing + aggregator tests pass.
- `dotnet run --project backend/SkyRoute.API` and hit:
  - `GET http://localhost:5000/api/airports` → 7 airports.
  - `POST /api/flights/search` body `{ "originAirportCode":"EZE","destinationAirportCode":"MIA","departureDate":"2026-08-01","numberOfPassengers":2,"cabinClass":0 }` → array containing both `GlobalAir` and `BudgetWings` entries with correct `TotalPrice = PricePerPassenger * 2`.
  - `POST /api/bookings` with a passenger using `DocumentType.NationalId` on the EZE→MIA route → **400 Bad Request** (international requires Passport).
  - Same booking with `DocumentType.Passport` → **200 OK** with `BookingReference` matching `SKY-2026-[A-F0-9]{6}`.
  - `GET /api/bookings/{reference}` returns the persisted booking.
  - Visit `/swagger` in dev for browsable API.
