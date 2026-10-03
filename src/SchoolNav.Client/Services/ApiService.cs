using System.Net.Http.Json;
using SchoolNav.Client.Models;

namespace SchoolNav.Client.Services;

public class ApiService
{
    private readonly HttpClient _http;
    public ApiService(HttpClient http) => _http = http;

    public async Task<List<LocationDto>> GetLocationsAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search) ? "api/locations" : $"api/locations?search={Uri.EscapeDataString(search)}";
        return await _http.GetFromJsonAsync<List<LocationDto>>(url) ?? new();
    }

    public async Task<List<NodeDto>> GetNodesAsync() =>
        await _http.GetFromJsonAsync<List<NodeDto>>("api/nodes") ?? new();

    public async Task<List<EdgeDto>> GetEdgesAsync() =>
        await _http.GetFromJsonAsync<List<EdgeDto>>("api/edges") ?? new();

    public async Task<RouteResultDto?> GetRouteAsync(int fromId, int toId)
    {
        var response = await _http.GetAsync($"api/route?fromId={fromId}&toId={toId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RouteResultDto>();
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }

    public async Task<bool> CreateLocationAsync(LocationCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/locations", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateLocationAsync(int id, LocationCreateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/locations/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteLocationAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/locations/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<NodeDto?> CreateNodeAsync(NodeCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/nodes", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<NodeDto>();
    }

    public async Task<bool> CreateEdgeAsync(EdgeCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/edges", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool success, string? error)> DeleteNodeAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/nodes/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(body) ? "Delete failed." : body);
    }

    public async Task<bool> DeleteEdgeAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/edges/{id}");
        return response.IsSuccessStatusCode;
    }
}
