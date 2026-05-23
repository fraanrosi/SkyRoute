using Microsoft.AspNetCore.Mvc;
using SkyRoute.API.Data;
using SkyRoute.API.Models.Dtos;

namespace SkyRoute.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirportsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Airport>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Airport>> GetAll()
        => Ok(AirportSeed.Airports);
}
