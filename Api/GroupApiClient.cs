using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Restaurant_Aqa_Framework.Api;

public sealed class GroupApiClient : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public GroupApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
    }

    // ------------------------------------------------------------------ Auth
    public static async Task<OwnerAccount> RegisterAndSignInAsync(string baseUrl, string prefix)
    {
        using var api = new GroupApiClient(baseUrl);
        var email = $"{prefix}{Guid.NewGuid():N}@example.com";
        const string password = "Passw0rd!123";

        var reg = await api.SendAsync(HttpMethod.Post, "register", new
        {
            firstName = "Aqa",
            lastName = "Tester",
            email,
            password,
            confirmPassword = password,
        });
        Ensure(reg, "register");

        var login = await api.SendAsync(HttpMethod.Post, "signin", new { email, password });
        Ensure(login, "signin");

        using var doc = JsonDocument.Parse(login.Body);
        var idToken = doc.RootElement.GetProperty("idToken").GetString()!;
        return new OwnerAccount(email, idToken, ReadSub(idToken));
    }

    public static async Task<string> SignInAsync(string baseUrl, string email, string password)
    {
        using var api = new GroupApiClient(baseUrl);
        var login = await api.SendAsync(HttpMethod.Post, "signin", new { email, password });
        Ensure(login, "signin");
        using var doc = JsonDocument.Parse(login.Body);
        return doc.RootElement.GetProperty("idToken").GetString()!;
    }

    // ----------------------------------------------------------- Підготовка даних
    public async Task<string> CreateReservationAsync(string ownerToken, string tableId, DateTime startUtc, DateTime endUtc, int guests)
    {
        var r = await SendAsync(HttpMethod.Post, "reservations", new
        {
            tableId,
            startTime = startUtc.ToString("o"),
            endTime = endUtc.ToString("o"),
            guestCount = guests,
        }, ownerToken);
        Ensure(r, "create reservation");
        using var doc = JsonDocument.Parse(r.Body);
        return FirstString(doc.RootElement, "id", "Id", "reservationId", "ReservationId")
               ?? throw new InvalidOperationException($"Reservation id not found in: {r.Body}");
    }

    public async Task<InviteInfo> GetInviteAsync(string ownerToken, string reservationId)
    {
        var t = await SendAsync(HttpMethod.Get, $"reservations/{reservationId}/invite-token", null, ownerToken);
        Ensure(t, "invite-token");
        using var tdoc = JsonDocument.Parse(t.Body);
        var token = FirstString(tdoc.RootElement, "token", "Token")
                    ?? throw new InvalidOperationException($"Invite token not found in: {t.Body}");

        var i = await SendAsync(HttpMethod.Get, $"guest/invites/{token}");
        Ensure(i, "guest invite");
        using var idoc = JsonDocument.Parse(i.Body);
        var groupId = FirstString(idoc.RootElement, "GroupReservationId", "groupReservationId")
                      ?? throw new InvalidOperationException($"GroupReservationId not found in: {i.Body}");
        var max = idoc.RootElement.TryGetProperty("MaxGuestCount", out var m) ? m.GetInt32() : 0;
        return new InviteInfo(groupId, max);
    }

    public async Task<GuestSession> CreateGuestSessionAsync(string groupId, string guestName)
    {
        var r = await SendAsync(HttpMethod.Post, "guest/sessions", new { groupReservationId = groupId, guestName });
        Ensure(r, "create guest session");
        using var doc = JsonDocument.Parse(r.Body);
        var id = FirstString(doc.RootElement, "SessionId", "sessionId")!;
        var token = FirstString(doc.RootElement, "SessionToken", "sessionToken")!;
        return new GuestSession(id, token);
    }

    public async Task<List<Dish>> GetSelectableDishesAsync(GuestSession session)
    {
        var r = await SendAsync(HttpMethod.Get, $"guest/sessions/{session.SessionId}/menu", null, session.SessionToken);
        Ensure(r, "guest menu");
        var result = new List<Dish>();
        using var doc = JsonDocument.Parse(r.Body);
        foreach (var category in doc.RootElement.GetProperty("dishes").EnumerateObject())
        {
            foreach (var d in category.Value.EnumerateArray())
            {
                var selectable = !d.TryGetProperty("selectable", out var s) || s.GetBoolean();
                if (selectable)
                {
                    result.Add(new Dish(d.GetProperty("id").GetString()!, d.GetProperty("name").GetString()!, d.GetProperty("price").GetDecimal()));
                }
            }
        }
        return result;
    }

    public async Task AddDishAsync(GuestSession session, string dishId, int quantity)
        => Ensure(await SendAsync(HttpMethod.Post, $"guest/sessions/{session.SessionId}/dishes", new { dishId, quantity }, session.SessionToken), "add dish");

    public async Task SubmitCartAsync(GuestSession session)
        => Ensure(await SendAsync(HttpMethod.Post, $"guest/sessions/{session.SessionId}/submit", new { }, session.SessionToken), "submit cart");

    public async Task CancelReservationAsync(string ownerToken, string reservationId)
        => await SendAsync(HttpMethod.Delete, "reservations", new { id = reservationId }, ownerToken);

    // ----------------------------------------------------------- Що тестуємо
    public async Task<ApiResult> RemoveSessionAsync(string groupId, string sessionId, string? bearerToken)
        => await SendAsync(HttpMethod.Delete, $"guest/groups/{groupId}/sessions/{sessionId}", null, bearerToken);

    public async Task<GroupView> GetSessionsAsync(string groupId, string? bearerToken = null)
    {
        var r = await SendAsync(HttpMethod.Get, $"guest/groups/{groupId}/sessions", null, bearerToken);
        Ensure(r, "get sessions");
        return JsonSerializer.Deserialize<GroupView>(r.Body, Json)!;
    }

    public async Task<string> GetStatusAsync(string groupId, string? bearerToken = null)
    {
        var r = await SendAsync(HttpMethod.Get, $"guest/groups/{groupId}/status", null, bearerToken);
        Ensure(r, "get status");
        using var doc = JsonDocument.Parse(r.Body);
        return doc.RootElement.GetProperty("status").GetString()!;
    }

    public async Task<ApiResult> GetOrderAsync(string token, string reservationId)
        => await SendAsync(HttpMethod.Get, $"orders/{reservationId}", null, token);

    // ------------------------------------------------------------------ helpers
    private async Task<ApiResult> SendAsync(HttpMethod method, string path, object? body = null, string? bearerToken = null)
    {
        using var req = new HttpRequestMessage(method, path);

        // Додаємо Authorization тільки якщо передано повноцінний JWT (з крапками)
        if (!string.IsNullOrWhiteSpace(bearerToken) && bearerToken.Contains('.'))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        if (body is not null)
        {
            req.Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");
        }
        using var resp = await _http.SendAsync(req);
        return new ApiResult(resp.StatusCode, await resp.Content.ReadAsStringAsync());
    }

    private static void Ensure(ApiResult r, string what)
    {
        if ((int)r.Status is < 200 or >= 300)
        {
            throw new InvalidOperationException($"{what} failed: {(int)r.Status} {r.Body}");
        }
    }

    private static string? FirstString(JsonElement el, params string[] names)
    {
        foreach (var n in names)
        {
            if (el.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.String)
            {
                return v.GetString();
            }
        }
        return null;
    }

    private static string ReadSub(string jwt)
    {
        var p = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        p = p.PadRight(p.Length + (4 - p.Length % 4) % 4, '=');
        using var doc = JsonDocument.Parse(Convert.FromBase64String(p));
        return doc.RootElement.GetProperty("sub").GetString()!;
    }

    public void Dispose() => _http.Dispose();

    // ------------------------------------------------------------------ моделі
    public sealed record ApiResult(HttpStatusCode Status, string Body);
    public sealed record OwnerAccount(string Email, string IdToken, string UserId);
    public sealed record InviteInfo(string GroupId, int MaxGuests);
    public sealed record GuestSession(string SessionId, string SessionToken);
    public sealed record Dish(string Id, string Name, decimal Price);
    public sealed record Participant(string Id, string Type, string Name, bool IsSubmitted, decimal Total);
    public sealed record GroupView(List<Participant> Participants, decimal Total);
}