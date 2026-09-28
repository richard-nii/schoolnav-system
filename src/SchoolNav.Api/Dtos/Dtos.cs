using SchoolNav.Api.Models;

namespace SchoolNav.Api.Dtos;

public record LocationDto(
    int Id,
    string Name,
    LocationCategory Category,
    string? Description,
    double Latitude,
    double Longitude,
    int NearestNodeId
);

public record LocationCreateDto(
    string Name,
    LocationCategory Category,
    string? Description,
    double Latitude,
    double Longitude,
    int NearestNodeId
);

public record NodeDto(int Id, double Latitude, double Longitude, NodeType Type, string? Label);

public record NodeCreateDto(double Latitude, double Longitude, NodeType Type, string? Label);

public record EdgeDto(int Id, int NodeAId, int NodeBId, double DistanceMeters);

public record EdgeCreateDto(int NodeAId, int NodeBId);

// A single point the frontend draws as part of the route polyline.
public record RoutePoint(double Latitude, double Longitude, string? Label);

public record RouteResultDto(
    bool Found,
    double TotalDistanceMeters,
    List<RoutePoint> Path
);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(string Token, DateTime ExpiresAtUtc, string FullName, IList<string> Roles);
