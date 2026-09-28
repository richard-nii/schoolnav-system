namespace SchoolNav.Api.Models;

public enum NodeType
{
    Waypoint,
    Entrance,
    Junction
}

// A Node is a point on a walkway (NOT a searchable destination).
// Together with Edges, Nodes form the graph that Dijkstra runs over.
public class Node
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public NodeType Type { get; set; } = NodeType.Waypoint;
    public string? Label { get; set; }

    public List<Edge> EdgesFrom { get; set; } = new();
    public List<Edge> EdgesTo { get; set; } = new();
}
