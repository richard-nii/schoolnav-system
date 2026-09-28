using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolNav.Api.Data;
using SchoolNav.Api.Dtos;
using SchoolNav.Api.Models;
using SchoolNav.Api.Services;

namespace SchoolNav.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EdgesController : ControllerBase
{
    private readonly AppDbContext _context;
    public EdgesController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EdgeDto>>> GetAll()
    {
        var edges = await _context.Edges
            .Select(e => new EdgeDto(e.Id, e.NodeAId, e.NodeBId, e.DistanceMeters))
            .ToListAsync();
        return Ok(edges);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EdgeDto>> Create(EdgeCreateDto dto)
    {
        var nodeA = await _context.Nodes.FindAsync(dto.NodeAId);
        var nodeB = await _context.Nodes.FindAsync(dto.NodeBId);
        if (nodeA is null || nodeB is null) return BadRequest("Both nodes must exist.");

        var edge = new Edge
        {
            NodeAId = dto.NodeAId,
            NodeBId = dto.NodeBId,
            DistanceMeters = PathfindingService.HaversineMeters(nodeA.Latitude, nodeA.Longitude, nodeB.Latitude, nodeB.Longitude)
        };
        _context.Edges.Add(edge);
        await _context.SaveChangesAsync();
        return Ok(new EdgeDto(edge.Id, edge.NodeAId, edge.NodeBId, edge.DistanceMeters));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var edge = await _context.Edges.FindAsync(id);
        if (edge is null) return NotFound();
        _context.Edges.Remove(edge);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
