using System.Collections.Concurrent;
using SkyRoute.API.Models.Entities;

namespace SkyRoute.API.Data;

public class InMemoryBookingStore
{
    private readonly ConcurrentDictionary<string, Booking> _bookings = new();

    public void Add(Booking booking) => _bookings[booking.BookingReference] = booking;

    public Booking? Get(string bookingReference)
        => _bookings.TryGetValue(bookingReference, out var b) ? b : null;
}
