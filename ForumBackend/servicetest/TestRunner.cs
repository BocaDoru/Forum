using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace servicetest;

public interface ITestFlow
{
    Task RunAsync(TestRunner test);
}

public record HttpResponse(int Code, JsonElement Json);

public class TestRunner(string apiBaseUrl)
{
    private static readonly HttpClient s_http = new();
    private static readonly JsonSerializerOptions s_jsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };
    private static readonly JsonDocument s_nullJson = JsonDocument.Parse("null");

    private readonly string _apiBaseUrl = apiBaseUrl;

    private string? _bearer;

    public void SetBearer(string token) => _bearer = token;

    public async Task Step(string name, Func<Task> action)
    {
        try
        {
            await action();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FAILED");
            Console.WriteLine($"Step:{name} failed with Exception:{ex.GetType().Name}, Msg:{ex.Message}, Trace:{ex.StackTrace}.");
            Console.ResetColor();

            throw new Exception($"E2E test failed at step:{name}");
        }
    }

    public Task<HttpResponse> PostAsync(string url, object? payload = null) => RequestAsync(HttpMethod.Post, url, payload);
    public Task<HttpResponse> GetAsync(string url, object? payload = null) => RequestAsync(HttpMethod.Get, url, payload);
    public Task<HttpResponse> PutAsync(string url, object? payload = null) => RequestAsync(HttpMethod.Put, url, payload);
    public Task<HttpResponse> PatchAsync(string url, object? payload = null) => RequestAsync(HttpMethod.Patch, url, payload);
    public Task<HttpResponse> DeleteAsync(string url, object? payload = null) => RequestAsync(HttpMethod.Delete, url, payload);

    public async Task<HttpResponse> RequestAsync(HttpMethod httpMethod, string url, object? payload)
    {
        string cleanBase = _apiBaseUrl.TrimEnd('/');
        string cleanUrl = url.TrimStart('/');
        string fullUrl = $"{cleanBase}/{cleanUrl}";

        using HttpRequestMessage req = new(httpMethod, fullUrl);

        if (_bearer is not null)
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer);
        }

        payload ??= new { };

        using MemoryStream reqStream = new();
        await JsonSerializer.SerializeAsync(reqStream, payload, s_jsonOpts);
        reqStream.Position = 0;
        using StreamReader reader = new(reqStream);
        req.Content = new StringContent(await reader.ReadToEndAsync(), Encoding.UTF8, "application/json");

        using var response = await s_http.SendAsync(req);
        using var responseContent = await response.Content.ReadAsStreamAsync();

        if ((int)response.StatusCode >= 500)
        {
            using StreamReader responseReader = new(responseContent);
            string literal = JsonSerializer.Serialize(await responseReader.ReadToEndAsync());
            return new HttpResponse(500, JsonDocument.Parse(literal).RootElement.Clone());
        }

        HttpResponse httpResponse;
        if (responseContent.Length != 0)
        {
            var responseJson = await JsonSerializer.DeserializeAsync<JsonElement>(responseContent);
            httpResponse = new((int)response.StatusCode, responseJson);
        }
        else
        {
            httpResponse = new((int)response.StatusCode, s_nullJson.RootElement.Clone());
        }

        return httpResponse;
    }

    public void Assert(bool condition, string errorMessage)
    {
        if (!condition) 
            throw new Exception(errorMessage);
    }
}
