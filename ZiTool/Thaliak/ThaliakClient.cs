using System.Net.Http.Json;
using System.Text.Json;

namespace ZiTool.Thaliak;

public class ThaliakClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ThaliakClient()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://thaliak.xiv.dev");
        _client.DefaultRequestHeaders.Add("User-Agent", "ZiTool");

        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }

    public async Task<List<Repository>> GetRepositories()
    {
        return await _client.GetFromJsonAsync<List<Repository>>("/api/repositories", _jsonOptions);
    }
}
