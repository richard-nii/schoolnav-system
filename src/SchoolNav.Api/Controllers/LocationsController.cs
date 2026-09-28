using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolNav.Api.Data;
using SchoolNav.Api.Dtos;
using SchoolNav.Api.Models;

namespace SchoolNav.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _context;
    public LocationsController(AppDbContext context) => _context = context;

    // GET /api/locations?search=library
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetAll([FromQuery] string? search)
    {
        var query = _context.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Name.Contains(search) || (l.Description != null && l.Description.Contains(search)));

        var results = await query
            .OrderBy(l => l.Name)
            .Select(l => new LocationDto(l.Id, l.Name, l.Category, l.Description, l.Latitude, l.Longitude, l.NearestNodeId))
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var l = await _context.Locations.FindAsync(id);
        if (l is null) return NotFound();
        return Ok(new LocationDto(l.Id, l.Name, l.Category, l.Description, l.Latitude, l.Longitude, l.NearestNodeId));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LocationDto>> Create(LocationCreateDto dto)
    {
        var location = new Location
        {
            Name = dto.Name,
            Category = dto.Category,
            Description = dto.Description,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            NearestNodeId = dto.NearestNodeId
        };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var result = new LocationDto(location.Id, location.Name, location.Category, location.Description,
            location.Latitude, location.Longitude, location.NearestNodeId);
        return CreatedAtAction(nameof(GetById), new { id = location.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, LocationCreateDto dto)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location is null) return NotFound();

        location.Name = dto.Name;
        location.Category = dto.Category;
        location.Description = dto.Description;
        location.Latitude = dto.Latitude;
        location.Longitude = dto.Longitude;
        location.NearestNodeId = dto.NearestNodeId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location is null) return NotFound();

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
