using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolNav.Api.Data;
using SchoolNav.Api.Dtos;
using SchoolNav.Api.Models;

namespace SchoolNav.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NodesController : ControllerBase
{
    private readonly AppDbContext _context;
    public NodesController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NodeDto>>> GetAll()
    {
        var nodes = await _context.Nodes
            .Select(n => new NodeDto(n.Id, n.Latitude, n.Longitude, n.Type, n.Label))
            .ToListAsync();
        return Ok(nodes);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NodeDto>> Create(NodeCreateDto dto)
    {
        var node = new Node { Latitude = dto.Latitude, Longitude = dto.Longitude, Type = dto.Type, Label = dto.Label };
        _context.Nodes.Add(node);
        await _context.SaveChangesAsync();
        return Ok(new NodeDto(node.Id, node.Latitude, node.Longitude, node.Type, node.Label));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var node = await _context.Nodes.FindAsync(id);
        if (node is null) return NotFound();

        bool inUse = await _context.Edges.AnyAsync(e => e.NodeAId == id || e.NodeBId == id)
                  || await _context.Locations.AnyAsync(l => l.NearestNodeId == id);
        if (inUse)
            return Conflict("Cannot delete a node that is still referenced by edges or locations.");

        _context.Nodes.Remove(node);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
