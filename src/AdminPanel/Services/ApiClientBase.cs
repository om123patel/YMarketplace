using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminPanel.Services
{
    public abstract class ApiClientBase
    {
        protected readonly HttpClient Http;
        protected readonly ILogger Logger;

        protected static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        protected ApiClientBase(HttpClient http, ILogger logger)
        {
            Http = http;
            Logger = logger;
        }

        protected async Task<T?> GetAsync<T>(string url, string? token)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                Attach(req, token);
                return await Read<T>(await Http.SendAsync(req));
            }
            catch (Exception ex) { Logger.LogError(ex, "GET {Url}", url); return default; }
        }

        protected async Task<T?> PostAsync<T>(string url, object? body, string? token)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                Attach(req, token);
                if (body != null) req.Content = Serialize(body);
                return await Read<T>(await Http.SendAsync(req));
            }
            catch (Exception ex) { Logger.LogError(ex, "POST {Url}", url); return default; }
        }

        protected async Task<T?> PutAsync<T>(string url, object? body, string? token)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Put, url);
                Attach(req, token);
                if (body != null) req.Content = Serialize(body);
                return await Read<T>(await Http.SendAsync(req));
            }
            catch (Exception ex) { Logger.LogError(ex, "PUT {Url}", url); return default; }
        }

        protected async Task<T?> PatchAsync<T>(string url, object? body, string? token)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Patch, url);
                Attach(req, token);
                if (body != null) req.Content = Serialize(body);
                return await Read<T>(await Http.SendAsync(req));
            }
            catch (Exception ex) { Logger.LogError(ex, "PATCH {Url}", url); return default; }
        }

        protected async Task<T?> DeleteAsync<T>(string url, string? token)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Delete, url);
                Attach(req, token);
                return await Read<T>(await Http.SendAsync(req));
            }
            catch (Exception ex) { Logger.LogError(ex, "DELETE {Url}", url); return default; }
        }

        protected static string BuildQuery(Dictionary<string, string?> pairs)
        {
            var qs = string.Join("&", pairs
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}"));
            return qs.Length > 0 ? "?" + qs : "";
        }

        private static void Attach(HttpRequestMessage req, string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private static StringContent Serialize(object body)
            => new(JsonSerializer.Serialize(body, JsonOpts),
                   Encoding.UTF8, "application/json");

        private static async Task<T?> Read<T>(HttpResponseMessage res)
        {
            var json = await res.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, JsonOpts);
        }
    }
}