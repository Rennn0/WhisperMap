using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XcLib.Shared.Extensions;

public static class HttpExtension
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

    public static async Task<(T? model, string json)> MakePostAsync<T>(this HttpClient httpClient, object request,
        string url, CancellationToken ct)
    {
        HttpResponseMessage res =
            await httpClient.PostAsJsonAsync(url, request, JsonSerializerOptions, ct);
        string content = await res.Content.ReadAsStringAsync(ct);
        T? model = JsonSerializer.Deserialize<T>(content);
        return (model, content);
    }
}