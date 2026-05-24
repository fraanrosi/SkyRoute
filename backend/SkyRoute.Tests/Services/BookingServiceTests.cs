using Microsoft.Extensions.Logging.Abstractions;
using SkyRoute.API.Data;
using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Models.Entities;
using SkyRoute.API.Models.Enums;
using SkyRoute.API.Services;

namespace SkyRoute.Tests.Services;

public class BookingServiceTests
{
    private static (BookingService service, InMemoryFlightStore flights, InMemoryBookingStore bookings)
        BuildSubjectUnderTest()
    {
        var flights = new InMemoryFlightStore();
        var bookings = new InMemoryBookingStore();
        var service = new BookingService(flights, bookings, NullLogger<BookingService>.Instance);
        return (service, flights, bookings);
    }

    private static Flight SeedInternationalFlight(InMemoryFlightStore store)
    {
        var flight = new Flight
        {
            FlightId = "GlobalAir-EZE-MIA-20260801-0",
            AirlineProvider = "GlobalAir",
            FlightNumber = "GA100",
            OriginAirportCode = "EZE",
            DestinationAirportCode = "MIA",
            DepartureTime = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc),
            ArrivalTime = new DateTime(2026, 8, 1, 20, 0, 0, DateTimeKind.Utc),
            Duration = TimeSpan.FromHours(10),
            CabinClass = CabinClass.Economy,
            BaseFare = 400m,
            PricePerPassenger = 460m
        };
        store.Upsert(flight);
        return flight;
    }

    private static Flight SeedDomesticFlight(InMemoryFlightStore store)
    {
        var flight = new Flight
        {
            FlightId = "BudgetWings-AEP-COR-20260801-0",
            AirlineProvider = "BudgetWings",
            FlightNumber = "BW200",
            OriginAirportCode = "AEP",
            DestinationAirportCode = "COR",
            DepartureTime = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc),
            ArrivalTime = new DateTime(2026, 8, 1, 11, 0, 0, DateTimeKind.Utc),
            Duration = TimeSpan.FromHours(2),
            CabinClass = CabinClass.Economy,
            BaseFare = 100m,
            PricePerPassenger = 90m
        };
        store.Upsert(flight);
        return flight;
    }

    private static PassengerDto Passenger(string name, DocumentType type, string doc) => new()
    {
        FullName = name,
        Email = $"{name.Replace(' ', '.').ToLowerInvariant()}@example.com",
        DocumentNumber = doc,
        DocumentType = type
    };

    [Fact]
    public void Create_RejectsNationalId_OnInternationalRoute()
    {
        var (service, flights, _) = BuildSubjectUnderTest();
        var flight = SeedInternationalFlight(flights);

        var request = new BookingRequest
        {
            FlightId = flight.FlightId,
            ContactEmail = "contact@example.com",
            Passengers = new() { Passenger("Alice Roe", DocumentType.NationalId, "12345678") }
        };

        var ex = Assert.Throws<ArgumentException>(() => service.Create(request));
        Assert.Contains("Passport", ex.Message);
    }

    [Fact]
    public void Create_RejectsPassport_OnDomesticRoute()
    {
        var (service, flights, _) = BuildSubjectUnderTest();
        var flight = SeedDomesticFlight(flights);

        var request = new BookingRequest
        {
            FlightId = flight.FlightId,
            ContactEmail = "contact@example.com",
            Passengers = new() { Passenger("Bob Roe", DocumentType.Passport, "AB123456") }
        };

        var ex = Assert.Throws<ArgumentException>(() => service.Create(request));
        Assert.Contains("National ID", ex.Message);
    }

    [Fact]
    public void Create_RejectsMixedDocumentTypes_NamingTheOffender()
    {
        var (service, flights, _) = BuildSubjectUnderTest();
        var flight = SeedInternationalFlight(flights);

        var request = new BookingRequest
        {
            FlightId = flight.FlightId,
            ContactEmail = "contact@example.com",
            Passengers = new()
            {
                Passenger("Alice Ok",       DocumentType.Passport,  "AB123456"),
                Passenger("Bob Mismatched",  DocumentType.NationalId, "12345678")
            }
        };

        var ex = Assert.Throws<ArgumentException>(() => service.Create(request));
        Assert.Contains("Bob Mismatched", ex.Message);
        Assert.DoesNotContain("Alice Ok", ex.Message);
    }

    [Fact]
    public void Create_International_WithPassports_Succeeds_AndReturnsReferenceMatchingPattern()
    {
        var (service, flights, store) = BuildSubjectUnderTest();
        var flight = SeedInternationalFlight(flights);

        var request = new BookingRequest
        {
            FlightId = flight.FlightId,
            ContactEmail = "contact@example.com",
            Passengers = new()
            {
                Passenger("Alice", DocumentType.Passport, "AB123456"),
                Passenger("Bob",   DocumentType.Passport, "CD789012")
            }
        };

        var response = service.Create(request);

        Assert.Matches(@"^SKY-\d{4}-[A-F0-9]{6}$", response.BookingReference);
        Assert.Equal(BookingStatus.Confirmed, response.Status);
        Assert.Equal(flight.PricePerPassenger * 2, response.TotalPriceCharged);
        Assert.NotNull(store.Get(response.BookingReference));
    }

    [Fact]
    public void Create_Domestic_WithNationalIds_Succeeds()
    {
        var (service, flights, _) = BuildSubjectUnderTest();
        var flight = SeedDomesticFlight(flights);

        var request = new BookingRequest
        {
            FlightId = flight.FlightId,
            ContactEmail = "contact@example.com",
            Passengers = new() { Passenger("Carlos Ruiz", DocumentType.NationalId, "30123456") }
        };

        var response = service.Create(request);

        Assert.Equal(BookingStatus.Confirmed, response.Status);
        Assert.Equal(flight.PricePerPassenger, response.TotalPriceCharged);
    }

    [Fact]
    public void Create_ThrowsWhenFlightUnknown()
    {
        var (service, _, _) = BuildSubjectUnderTest();

        var request = new BookingRequest
        {
            FlightId = "Nope-XYZ",
            ContactEmail = "c@example.com",
            Passengers = new() { Passenger("Alice", DocumentType.Passport, "AB123456") }
        };

        var ex = Assert.Throws<ArgumentException>(() => service.Create(request));
        Assert.Contains("Flight not found", ex.Message);
    }

    [Fact]
    public void Create_ThrowsWhenPassengersListEmpty()
    {
        var (service, flights, _) = BuildSubjectUnderTest();
        var flight = SeedInternationalFlight(flights);

        var request = new BookingRequest
        {
            FlightId = flight.FlightId,
            ContactEmail = "c@example.com",
            Passengers = new()
        };

        Assert.Throws<ArgumentException>(() => service.Create(request));
    }

    [Fact]
    public void Get_ReturnsNull_WhenReferenceUnknown()
    {
        var (service, _, _) = BuildSubjectUnderTest();
        Assert.Null(service.Get("SKY-2026-NOPE99"));
    }

    [Fact]
    public void Get_RoundTripsAfterCreate()
    {
        var (service, flights, _) = BuildSubjectUnderTest();
        var flight = SeedDomesticFlight(flights);

        var created = service.Create(new BookingRequest
        {
            FlightId = flight.FlightId,
            ContactEmail = "c@example.com",
            Passengers = new() { Passenger("Carlos", DocumentType.NationalId, "30123456") }
        });

        var fetched = service.Get(created.BookingReference);
        Assert.NotNull(fetched);
        Assert.Equal(created.BookingReference, fetched!.BookingReference);
        Assert.Equal(created.TotalPriceCharged, fetched.TotalPriceCharged);
    }

    [Fact]
    public void GetAll_EnrichesWithFlightDataAndSortsByBookingDateDescending()
    {
        var (service, flights, _) = BuildSubjectUnderTest();
        var intl = SeedInternationalFlight(flights);
        var dom = SeedDomesticFlight(flights);

        // Older booking first.
        var first = service.Create(new BookingRequest
        {
            FlightId = dom.FlightId,
            ContactEmail = "c1@example.com",
            Passengers = new() { Passenger("Carlos", DocumentType.NationalId, "30111111") }
        });
        Thread.Sleep(15); // ensure distinct BookingDate values
        var second = service.Create(new BookingRequest
        {
            FlightId = intl.FlightId,
            ContactEmail = "c2@example.com",
            Passengers = new() { Passenger("Alice", DocumentType.Passport, "AB123456") }
        });

        var list = service.GetAll();

        Assert.Equal(2, list.Count);
        Assert.Equal(second.BookingReference, list[0].BookingReference); // newest first
        Assert.Equal(first.BookingReference, list[1].BookingReference);

        var newestItem = list[0];
        Assert.Equal("GlobalAir", newestItem.AirlineProvider);
        Assert.Equal("GA100", newestItem.FlightNumber);
        Assert.Equal("EZE", newestItem.OriginAirportCode);
        Assert.Equal("MIA", newestItem.DestinationAirportCode);
        Assert.Equal(1, newestItem.NumberOfPassengers);
        Assert.Equal("c2@example.com", newestItem.ContactEmail);
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoBookings()
    {
        var (service, _, _) = BuildSubjectUnderTest();
        Assert.Empty(service.GetAll());
    }
}
