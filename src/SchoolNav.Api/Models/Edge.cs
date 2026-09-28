namespace SchoolNav.Api.Models;

// An Edge is a walkable path segment between two Nodes.
// Edges are treated as bidirectional (you can walk a path both ways).
public class Edge
{
    public int Id { get; set; }

    public int NodeAId { get; set; }
    public Node? NodeA { get; set; }

    public int NodeBId { get; set; }
    public Node? NodeB { get; set; }

    // Weight used by Dijkstra. If not set explicitly, computed
    // automatically from the two nodes' GPS coordinates.
    public double DistanceMeters { get; set; }
}
