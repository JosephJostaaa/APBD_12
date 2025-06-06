using Microsoft.AspNetCore.Mvc;
using Task12.Dto;
using Task12.Services;

namespace Task12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _tripService.GetTripsAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] AssignClientToTripDto dto)
    {
        var result = await _tripService.AssignClientToTripAsync(idTrip, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return CreatedAtAction(nameof(AssignClientToTrip), new { idTrip = idTrip }, null);
    }
}
