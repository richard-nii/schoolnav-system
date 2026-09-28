using Microsoft.AspNetCore.Mvc;
using SchoolNav.Api.Dtos;
using SchoolNav.Api.Services;

namespace SchoolNav.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RouteController : ControllerBase
{
    private readonly PathfindingService _pathfindingService;
    public RouteController(PathfindingService pathfindingService) => _pathfindingService = pathfindingService;

    // GET /api/route?fromId=3&toId=7
    [HttpGet]
    public async Task<ActionResult<RouteResultDto>> GetRoute([FromQuery] int fromId, [FromQuery] int toId)
    {
        if (fromId == toId)
            return BadRequest("Start and destination must be different locations.");

        var result = await _pathfindingService.FindRouteAsync(fromId, toId);
        if (!result.Found)
            return NotFound(new RouteResultDto(false, 0, new List<RoutePoint>()));

        return Ok(result);
    }
}
