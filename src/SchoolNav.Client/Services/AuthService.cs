using System.Net.Http.Headers;
using Blazored.LocalStorage;
using SchoolNav.Client.Models;

namespace SchoolNav.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;

    public string? FullName { get; private set; }
    public List<string> Roles { get; private set; } = new();
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsLoggedIn => !string.IsNullOrEmpty(FullName);

    public AuthService(HttpClient http, ILocalStorageService localStorage)
    {
        _http = http;
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");
        var fullName = await _localStorage.GetItemAsStringAsync("authFullName");
        var rolesRaw = await _localStorage.GetItemAsync<List<string>>("authRoles");

        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(fullName))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            FullName = fullName;
            Roles = rolesRaw ?? new();
        }
    }

    public async Task<bool> LoginAsync(ApiService api, string email, string password)
    {
        var result = await api.LoginAsync(new LoginDto(email, password));
        if (result is null) return false;

        await _localStorage.SetItemAsStringAsync("authToken", result.Token);
        await _localStorage.SetItemAsStringAsync("authFullName", result.FullName);
        await _localStorage.SetItemAsync("authRoles", result.Roles);

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        FullName = result.FullName;
        Roles = result.Roles;
        return true;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("authFullName");
        await _localStorage.RemoveItemAsync("authRoles");
        _http.DefaultRequestHeaders.Authorization = null;
        FullName = null;
        Roles = new();
    }
}
