using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ogur.Terraria.Manager.Infrastructure.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _sessionId;
    private string? _currentUsername;
    private string? _currentRole;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionId);
    public string? CurrentUsername => _currentUsername;
    public string? CurrentRole => _currentRole;

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<(bool success, string? role, string? error)> LoginAsync(string username, string password)
    {
        try
        {
            var loginData = new { username, password };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                _sessionId = result.GetProperty("sessionId").GetString();
                _currentUsername = username;
                _currentRole = result.GetProperty("role").GetString();

                _httpClient.DefaultRequestHeaders.Remove("X-Session-Id");
                _httpClient.DefaultRequestHeaders.Add("X-Session-Id", _sessionId);

                Console.WriteLine($"✅ Login successful: {username} ({_currentRole})");
                return (true, _currentRole, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Login failed: {response.StatusCode} - {errorContent}");
            return (false, null, $"Login failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Login exception: {ex.Message}");
            return (false, null, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_sessionId))
            {
                await _httpClient.PostAsync("/api/auth/logout", null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Logout error: {ex.Message}");
        }
        finally
        {
            _sessionId = null;
            _currentUsername = null;
            _currentRole = null;
            _httpClient.DefaultRequestHeaders.Remove("X-Session-Id");
        }
    }

    public async Task<bool> ValidateSessionAsync()
    {
        if (string.IsNullOrEmpty(_sessionId))
            return false;

        try
        {
            var response = await _httpClient.GetAsync("/api/auth/validate");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
