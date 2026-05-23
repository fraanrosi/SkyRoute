# SkyRoute — Frontend Plan (Angular 17, standalone)

> Companion to [backend-plan.md](./backend-plan.md).

## Context

This plan scaffolds the Angular 17 frontend that consumes the .NET API: a search form with provider-aggregated results, frontend-only sorting, and a booking flow whose document field reactively switches between **Passport** (international) and **National ID** (domestic) based on the selected flight's route.

**Outcome:** an Angular standalone app running on `http://localhost:4200`, proxying `/api` to the .NET backend, fully exercising the search → results → booking → confirmation happy path plus loading/empty/error states.

**Repo layout (frontend slice):**

```
SkyRoute/
└── frontend/
    └── skyroute-web/              # Angular 17 standalone workspace
```

---

## B1. Workspace bootstrap
- `cd frontend && ng new skyroute-web --standalone --routing --style=scss --skip-git`.
- Angular 17+, no NgModules.
- Configure `proxy.conf.json` mapping `/api` → `http://localhost:5000` (or whatever the API port resolves to) so the dev server hits the .NET API without CORS surprises (CORS is still wired server-side as belt-and-suspenders).

## B2. Routing — `src/app/app.routes.ts`
```
''                                 → redirect to /search
'search'                           → SearchPageComponent              (form + results)
'booking/:flightId'                → BookingPageComponent             (passenger FormArray)
'booking/confirmation/:reference'  → BookingConfirmationComponent
**                                 → redirect to /search
```

## B3. Shared layer — `src/app/shared/`
- `services/airport.service.ts` — `getAirports(): Observable<Airport[]>`; cached after first call.
- `validators/passport.validator.ts` — pattern `^[A-Z0-9]{6,9}$`.
- `validators/national-id.validator.ts` — pattern `^\d{7,8}$`.
- `models/shared.models.ts` — `Airport`, `CabinClass`, `DocumentType` enums mirroring backend.
- Helper: `isInternational(originCountry, destinationCountry): boolean`.

## B4. Flight-search feature — `src/app/features/flight-search/`
- `services/flight-search.service.ts` — `search(req): Observable<FlightResult[]>` hitting `POST /api/flights/search`; holds last results in a `signal<FlightResult[]>` for sort/no-refetch.
- `components/search-form.component.ts` — Reactive Form: origin (select), destination (select), departureDate (date, future only), numberOfPassengers (1–9), cabinClass (select). Submitting emits search request.
- `components/flight-results.component.ts` — owns the sort `signal<'priceAsc'|'priceDesc'|'durationAsc'|'departureAsc'>` and a `computed` sorted list. Renders **loading**, **empty**, and **error** states explicitly.
- `components/flight-card.component.ts` — shows airline, flight number, times, duration, price-per-passenger × N = total, with a "Book" button that routes to `/booking/:flightId` (passing the flight via service cache, not URL).
- `models/flight.models.ts` — `FlightSearchRequest`, `FlightResult`.

## B5. Booking feature — `src/app/features/booking/`
- `services/booking.service.ts` — `createBooking(req): Observable<BookingResponse>`, `getBooking(reference)`, plus holds the selected `FlightResult` chosen from the search results.
- `components/booking-page.component.ts` — top-level Reactive Form:
  - `contactEmail: FormControl`
  - `passengers: FormArray` with exactly `NumberOfPassengers` groups, each: `{ fullName, email, documentNumber }`.
  - Reads the selected flight from the service; derives `isInternational` from the flight's origin/destination airports (looked up via `AirportService`).
  - Reactively swaps the document-field label ("Passport Number" vs "National ID") and the attached validator on every passenger group whenever the route type changes (note: route doesn't change mid-form in practice, but the wiring is reactive so it's robust if user navigates back).
  - On submit: builds `BookingRequest` (sets `DocumentType` to `Passport` for international, `NationalId` for domestic), calls service, navigates to `/booking/confirmation/:reference`.
- `components/passenger-form.component.ts` — child component receiving the passenger `FormGroup` and the current document-type label/validator from the parent.
- `components/booking-confirmation.component.ts` — shows booking reference, flight summary, total charged, status; fetches via `getBooking(reference)` if state was lost.
- `models/booking.models.ts` — `BookingRequest`, `PassengerDto`, `BookingResponse`.

## B6. UX details
- Loading spinners on search + booking submit.
- Empty state on zero results.
- Error toast/banner on API failures.
- Frontend-only sorting — no extra API call.
- Form-level validation messages surfaced inline.

## B7. Critical files
- [frontend/skyroute-web/src/app/app.routes.ts](../frontend/skyroute-web/src/app/app.routes.ts)
- [frontend/skyroute-web/src/app/features/flight-search/components/search-form.component.ts](../frontend/skyroute-web/src/app/features/flight-search/components/search-form.component.ts)
- [frontend/skyroute-web/src/app/features/flight-search/components/flight-results.component.ts](../frontend/skyroute-web/src/app/features/flight-search/components/flight-results.component.ts)
- [frontend/skyroute-web/src/app/features/booking/components/booking-page.component.ts](../frontend/skyroute-web/src/app/features/booking/components/booking-page.component.ts)
- [frontend/skyroute-web/src/app/shared/validators/passport.validator.ts](../frontend/skyroute-web/src/app/shared/validators/passport.validator.ts)
- [frontend/skyroute-web/src/app/shared/validators/national-id.validator.ts](../frontend/skyroute-web/src/app/shared/validators/national-id.validator.ts)

---

## Verification

- `ng serve`, open `http://localhost:4200`.
- Search EZE→MIA, 2 passengers — both providers visible, sort toggles update order with no network call.
- Click Book — booking page shows **2** passenger forms, document field labeled **"Passport Number"** with passport regex.
- Search AEP→COR (domestic) → label switches to **"National ID"** with the DNI regex.
- Submit → confirmation page with booking reference; backend `GET /api/bookings/{reference}` confirms persistence.
