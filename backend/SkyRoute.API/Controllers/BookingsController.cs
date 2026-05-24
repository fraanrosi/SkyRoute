using Microsoft.AspNetCore.Mvc;
using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Services;

namespace SkyRoute.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingService _bookingService;

    public BookingsController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<BookingResponse> Create([FromBody] BookingRequest request)
    {
        try
        {
            var response = _bookingService.Create(request);
            return CreatedAtAction(nameof(GetByReference), new { reference = response.BookingReference }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{reference}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookingResponse> GetByReference(string reference)
    {
        var response = _bookingService.Get(reference);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookingListItem>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<BookingListItem>> GetAll()
        => Ok(_bookingService.GetAll());
}
