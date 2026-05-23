using Microsoft.AspNetCore.Mvc;
using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Services;

namespace SkyRoute.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly FlightAggregatorService _aggregator;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(FlightAggregatorService aggregator, ILogger<FlightsController> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(IEnumerable<FlightResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<FlightResult>>> Search(
        [FromBody] FlightSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DepartureDate.Date < DateTime.UtcNow.Date)
            return BadRequest(new { error = "Departure date must be today or later." });

        try
        {
            var results = await _aggregator.SearchAsync(request, cancellationToken);
            _logger.LogInformation("Search {Origin}->{Destination} returned {Count} flights",
                request.OriginAirportCode, request.DestinationAirportCode, results.Count);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
