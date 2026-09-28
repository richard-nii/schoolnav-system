using System.ComponentModel.DataAnnotations;

namespace SchoolNav.Api.Models;

public enum LocationCategory
{
    Classroom,
    Office,
    Library,
    Hall,
    Laboratory,
    Cafeteria,
    SportsFacility,
    AdminBuilding,
    Other
}

public class Location
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public LocationCategory Category { get; set; } = LocationCategory.Other;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    // The nearest walkway graph node this location connects to,
    // used as the entry/exit point for route calculations.
    public int NearestNodeId { get; set; }
    public Node? NearestNode { get; set; }
}
