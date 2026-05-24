using SkyRoute.API.Data;
using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Models.Entities;
using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Services;

public class BookingService
{
    private readonly InMemoryFlightStore _flightStore;
    private readonly InMemoryBookingStore _bookingStore;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        InMemoryFlightStore flightStore,
        InMemoryBookingStore bookingStore,
        ILogger<BookingService> logger)
    {
        _flightStore = flightStore;
        _bookingStore = bookingStore;
        _logger = logger;
    }

    public BookingResponse Create(BookingRequest request)
    {
        if (request.Passengers is null || request.Passengers.Count == 0)
            throw new ArgumentException("At least one passenger is required.");

        var flight = _flightStore.Get(request.FlightId)
            ?? throw new ArgumentException($"Flight not found: {request.FlightId}. Run a search first.");

        var isInternational = AirportSeed.IsInternational(flight.OriginAirportCode, flight.DestinationAirportCode);
        var expectedDocType = isInternational ? DocumentType.Passport : DocumentType.NationalId;

        var mismatched = request.Passengers
            .Where(p => p.DocumentType != expectedDocType)
            .Select(p => p.FullName)
            .ToList();

        if (mismatched.Count > 0)
        {
            throw new ArgumentException(
                isInternational
                    ? $"International route requires Passport for all passengers. Offending: {string.Join(", ", mismatched)}."
                    : $"Domestic route requires National ID for all passengers. Offending: {string.Join(", ", mismatched)}.");
        }

        var booking = new Booking
        {
            BookingReference = GenerateReference(),
            BookingDate = DateTime.UtcNow,
            FlightId = flight.FlightId,
            Passengers = request.Passengers,
            ContactEmail = request.ContactEmail,
            TotalPriceCharged = flight.PricePerPassenger * request.Passengers.Count,
            Status = BookingStatus.Confirmed
        };

        _bookingStore.Add(booking);
        _logger.LogInformation("Booking {Reference} confirmed for flight {FlightId} ({Passengers} passengers, total {Total})",
            booking.BookingReference, booking.FlightId, booking.Passengers.Count, booking.TotalPriceCharged);

        return ToResponse(booking);
    }

    public BookingResponse? Get(string bookingReference)
    {
        var booking = _bookingStore.Get(bookingReference);
        return booking is null ? null : ToResponse(booking);
    }

    public IReadOnlyList<BookingListItem> GetAll()
    {
        return _bookingStore.GetAll()
            .Select(ToListItem)
            .OrderByDescending(b => b.BookingDate)
            .ToList();
    }

    private BookingListItem ToListItem(Booking booking)
    {
        var flight = _flightStore.Get(booking.FlightId);
        return new BookingListItem
        {
            BookingReference = booking.BookingReference,
            BookingDate = booking.BookingDate,
            Status = booking.Status,
            TotalPriceCharged = booking.TotalPriceCharged,
            AirlineProvider = flight?.AirlineProvider ?? "Unknown",
            FlightNumber = flight?.FlightNumber ?? string.Empty,
            OriginAirportCode = flight?.OriginAirportCode ?? string.Empty,
            DestinationAirportCode = flight?.DestinationAirportCode ?? string.Empty,
            DepartureTime = flight?.DepartureTime ?? default,
            NumberOfPassengers = booking.Passengers.Count,
            ContactEmail = booking.ContactEmail
        };
    }

    private static BookingResponse ToResponse(Booking booking) => new()
    {
        BookingReference = booking.BookingReference,
        BookingDate = booking.BookingDate,
        FlightId = booking.FlightId,
        TotalPriceCharged = booking.TotalPriceCharged,
        Status = booking.Status
    };

    private static string GenerateReference()
        => $"SKY-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}
