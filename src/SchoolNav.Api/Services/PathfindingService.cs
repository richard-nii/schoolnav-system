using SchoolNav.Api.Data;
using SchoolNav.Api.Dtos;
using SchoolNav.Api.Models;

namespace SchoolNav.Api.Services;

public class PathfindingService
{
    private readonly AppDbContext _context;

    public PathfindingService(AppDbContext context)
    {
        _context = context;
    }

    // Straight-line distance between two GPS points, in meters.
    public static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6_371_000;
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180.0;

    // Finds the shortest walking route between two Locations, routing through
    // the Node/Edge graph via Dijkstra's algorithm, and returns it as an
    // ordered list of GPS points ready to be drawn as a polyline.
    public async Task<RouteResultDto> FindRouteAsync(int fromLocationId, int toLocationId)
    {
        var fromLocation = await _context.Locations.FindAsync(fromLocationId);
        var toLocation = await _context.Locations.FindAsync(toLocationId);
        if (fromLocation is null || toLocation is null)
            return new RouteResultDto(false, 0, new List<RoutePoint>());

        var nodes = await Task.Run(() => _context.Nodes.ToList());
        var edges = await Task.Run(() => _context.Edges.ToList());

        var adjacency = BuildAdjacency(nodes, edges);

        var (path, distance) = RunDijkstra(adjacency, fromLocation.NearestNodeId, toLocation.NearestNodeId);
        if (path.Count == 0)
            return new RouteResultDto(false, 0, new List<RoutePoint>());

        var nodeLookup = nodes.ToDictionary(n => n.Id);

        var fullPath = new List<RoutePoint>
        {
            new(fromLocation.Latitude, fromLocation.Longitude, fromLocation.Name)
        };

        foreach (var nodeId in path)
        {
            var node = nodeLookup[nodeId];
            fullPath.Add(new RoutePoint(node.Latitude, node.Longitude, node.Label));
        }

        fullPath.Add(new RoutePoint(toLocation.Latitude, toLocation.Longitude, toLocation.Name));

        // Add the short "last mile" hops from each location to its nearest node.
        double startSpur = HaversineMeters(fromLocation.Latitude, fromLocation.Longitude,
            nodeLookup[fromLocation.NearestNodeId].Latitude, nodeLookup[fromLocation.NearestNodeId].Longitude);
        double endSpur = HaversineMeters(toLocation.Latitude, toLocation.Longitude,
            nodeLookup[toLocation.NearestNodeId].Latitude, nodeLookup[toLocation.NearestNodeId].Longitude);

        return new RouteResultDto(true, distance + startSpur + endSpur, fullPath);
    }

    private static Dictionary<int, List<(int neighborId, double weight)>> BuildAdjacency(
        List<Node> nodes, List<Edge> edges)
    {
        var adjacency = nodes.ToDictionary(n => n.Id, _ => new List<(int, double)>());

        foreach (var edge in edges)
        {
            var weight = edge.DistanceMeters > 0
                ? edge.DistanceMeters
                : HaversineMetersForNodes(nodes, edge.NodeAId, edge.NodeBId);

            // Treat every edge as bidirectional.
            adjacency[edge.NodeAId].Add((edge.NodeBId, weight));
            adjacency[edge.NodeBId].Add((edge.NodeAId, weight));
        }

        return adjacency;
    }

    private static double HaversineMetersForNodes(List<Node> nodes, int aId, int bId)
    {
        var a = nodes.First(n => n.Id == aId);
        var b = nodes.First(n => n.Id == bId);
        return HaversineMeters(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
    }

    // Classic Dijkstra shortest-path over the node graph.
    // Returns the ordered list of node ids from start to end (inclusive) and total distance.
    private static (List<int> path, double distance) RunDijkstra(
        Dictionary<int, List<(int neighborId, double weight)>> adjacency,
        int startNodeId, int endNodeId)
    {
        var distances = adjacency.Keys.ToDictionary(id => id, _ => double.PositiveInfinity);
        var previous = new Dictionary<int, int?>();
        var visited = new HashSet<int>();

        if (!distances.ContainsKey(startNodeId) || !distances.ContainsKey(endNodeId))
            return (new List<int>(), 0);

        distances[startNodeId] = 0;
        var priorityQueue = new SortedSet<(double dist, int id)>(Comparer<(double dist, int id)>.Create(
            (a, b) => a.dist != b.dist ? a.dist.CompareTo(b.dist) : a.id.CompareTo(b.id)));
        priorityQueue.Add((0, startNodeId));

        while (priorityQueue.Count > 0)
        {
            var (currentDist, currentId) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);

            if (!visited.Add(currentId)) continue;
            if (currentId == endNodeId) break;

            foreach (var (neighborId, weight) in adjacency[currentId])
            {
                if (visited.Contains(neighborId)) continue;

                var newDist = currentDist + weight;
                if (newDist < distances[neighborId])
                {
                    priorityQueue.Remove((distances[neighborId], neighborId));
                    distances[neighborId] = newDist;
                    previous[neighborId] = currentId;
                    priorityQueue.Add((newDist, neighborId));
                }
            }
        }

        if (double.IsPositiveInfinity(distances[endNodeId]))
            return (new List<int>(), 0); // no path found

        // Reconstruct path by walking backwards from end to start.
        var path = new List<int>();
        int? step = endNodeId;
        while (step is not null)
        {
            path.Insert(0, step.Value);
            previous.TryGetValue(step.Value, out var prev);
            step = step.Value == startNodeId ? null : prev;
        }

        return (path, distances[endNodeId]);
    }
}
