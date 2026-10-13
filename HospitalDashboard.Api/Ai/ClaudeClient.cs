using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HospitalDashboard.Api.Ai;

public class ClaudeClient(HttpClient http, IConfiguration config)
{
    public async Task<string> AskAsync(string system, string user, CancellationToken ct = default)
    {
        var apiKey = config["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey is not set.");
        var model = config["Anthropic:Model"] ?? "claude-sonnet-5-5";

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = JsonContent.Create(new
        {
            model,
            max_tokens = 1024,
            system,
            messages = new[] { new { role = "user", content = user } }
        });

        using var res = await http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Claude API returned {(int)res.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        var sb = new StringBuilder();
        foreach (var block in doc.RootElement.GetProperty("content").EnumerateArray())
        {
            if (block.TryGetProperty("type", out var type) && type.GetString() == "text"
                && block.TryGetProperty("text", out var text))
                sb.Append(text.GetString());
        }
        return sb.ToString();
    }
}