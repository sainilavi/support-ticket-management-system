using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SupportTicketManagementSystem.IntegrationTests;

internal static class HttpClientJsonExtensions
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    public static StringContent ToJsonContent<T>(this T payload) =>
        new(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string uri, T payload) =>
        client.PostAsync(uri, payload.ToJsonContent());

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string uri, T payload) =>
        client.PutAsync(uri, payload.ToJsonContent());
}
