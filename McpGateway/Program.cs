using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var http = new HttpClient();

// Change later when you add more services
var services = new[]
{
    new
    {
        AppId = "math-service",
        ToolsEndpoint = "http://localhost:3500/v1.0/invoke/math-service/method/__tools"
    }
};

app.MapPost("/", async (HttpRequest request) =>
{
    var json = await JsonDocument.ParseAsync(request.Body);
    var root = json.RootElement;

    var method = root.GetProperty("method").GetString();
    var id = root.GetProperty("id");

    // =========================
    // tools/list (AUTO)
    // =========================
    if (method == "tools/list")
    {
        var allTools = new List<object>();

        foreach (var svc in services)
        {
            var res = await http.GetStringAsync(svc.ToolsEndpoint);
            var tools = JsonSerializer.Deserialize<JsonElement>(res);

            foreach (var tool in tools.EnumerateArray())
            {
                allTools.Add(tool);
            }
        }

        return Results.Json(new
        {
            jsonrpc = "2.0",
            id,
            result = allTools
        });
    }

    // =========================
    // tools/call (AUTO)
    // =========================
    if (method == "tools/call")
    {
        var @params = root.GetProperty("params");
        var toolName = @params.GetProperty("name").GetString();
        var args = @params.GetProperty("arguments");

        // For now route everything to math-service
        // (Later: map tool → service dynamically)
        var query = string.Join("&",
            args.EnumerateObject()
                .Select(p => $"{p.Name}={p.Value.GetRawText()}"));

        var url =
            $"http://localhost:3500/v1.0/invoke/math-service/method/{toolName}?{query}";

        var response = await http.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        return Results.Json(new
        {
            jsonrpc = "2.0",
            id,
            result = JsonSerializer.Deserialize<object>(body)
        });
    }

    return Results.Json(new
    {
        jsonrpc = "2.0",
        id,
        error = new { code = -32601, message = "Method not found" }
    });
});

app.Run();
