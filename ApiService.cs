using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FitnessCenter.Client;

// ── Собственный тип делегата для обработки результатов API-запросов ──
public delegate void OnRequestCompleted(string endpoint, string method, int statusCode, long elapsedMs, string? responseBody);
public delegate void OnRequestError(string endpoint, string method, string errorMessage);

// ── DTOs (копии из API) ──
public record ClientDto(int Id, string FirstName, string LastName, string Email, string Phone,
    DateTime DateOfBirth, DateTime MembershipStart, DateTime MembershipEnd, bool IsActive);
public record CreateClientDto(string FirstName, string LastName, string Email, string Phone,
    DateTime DateOfBirth, DateTime MembershipStart, DateTime MembershipEnd);
public record TrainerDto(int Id, string FirstName, string LastName, string Email, string Phone,
    string Specialization, int ExperienceYears, bool IsActive);
public record FitnessClassDto(int Id, string Name, string Description, int DurationMinutes,
    int MaxParticipants, string Difficulty, int TrainerId, string? TrainerName);
public record ScheduleDto(int Id, int ClassId, string? ClassName, int ClientId,
    string? ClientName, DateTime StartTime, string Status, string? Notes);
public record CreateScheduleDto(int ClassId, int ClientId, DateTime StartTime, string? Notes);

public class ApiService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    // ── Многоадресные делегаты ──
    public event OnRequestCompleted? RequestCompleted;
    public event OnRequestError? RequestErrored;

    // ── Action<T> для логирования операций ──
    public Action<string>? OnLogMessage { get; set; }

    public ApiService(string baseUrl = "http://localhost/api")
    {
        _baseUrl = baseUrl;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    private async Task<(int statusCode, string body, long elapsedMs)> SendAsync(
        HttpMethod method, string path, object? body = null)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var request = new HttpRequestMessage(method, $"{_baseUrl}/{path}");
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            sw.Stop();
            RequestCompleted?.Invoke(path, method.Method, (int)response.StatusCode, sw.ElapsedMilliseconds, responseBody);
            OnLogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {method.Method} {path} → {(int)response.StatusCode} ({sw.ElapsedMilliseconds}ms)");
            return ((int)response.StatusCode, responseBody, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            RequestErrored?.Invoke(path, method.Method, ex.Message);
            OnLogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] ОШИБКА {method.Method} {path}: {ex.Message}");
            return (0, ex.Message, sw.ElapsedMilliseconds);
        }
    }

    // ── Func<T, TResult> для обёртки операций CRUD ──
    public Func<Task<List<ClientDto>?>> GetClientsFunc => async () =>
    {
        var (_, body, _) = await SendAsync(HttpMethod.Get, "clients");
        return JsonSerializer.Deserialize<List<ClientDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    };

    public Func<int, Task<ClientDto?>> GetClientByIdFunc => async (id) =>
    {
        var (status, body, _) = await SendAsync(HttpMethod.Get, $"clients/{id}");
        if (status == 404) return null;
        return JsonSerializer.Deserialize<ClientDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    };

    public Func<CreateClientDto, Task<ClientDto?>> CreateClientFunc => async (dto) =>
    {
        var (status, body, _) = await SendAsync(HttpMethod.Post, "clients", dto);
        if (status != 201) return null;
        return JsonSerializer.Deserialize<ClientDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    };

    // ── Прямые методы ──
    public async Task<List<TrainerDto>?> GetTrainersAsync()
    {
        var (_, body, _) = await SendAsync(HttpMethod.Get, "trainers");
        return JsonSerializer.Deserialize<List<TrainerDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<List<FitnessClassDto>?> GetClassesAsync()
    {
        var (_, body, _) = await SendAsync(HttpMethod.Get, "fitnessclasses");
        return JsonSerializer.Deserialize<List<FitnessClassDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<List<ScheduleDto>?> GetSchedulesAsync()
    {
        var (_, body, _) = await SendAsync(HttpMethod.Get, "schedules");
        return JsonSerializer.Deserialize<List<ScheduleDto>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<ScheduleDto?> CreateScheduleAsync(CreateScheduleDto dto)
    {
        var (status, body, _) = await SendAsync(HttpMethod.Post, "schedules", dto);
        if (status != 201) return null;
        return JsonSerializer.Deserialize<ScheduleDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var (status, _, _) = await SendAsync(HttpMethod.Delete, $"clients/{id}");
        return status == 204;
    }
}
