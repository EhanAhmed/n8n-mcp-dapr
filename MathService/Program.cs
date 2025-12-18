using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//
// ======================
// BUSINESS ENDPOINTS
// ======================
//

// Sum two numbers
app.MapGet("/sum", (int a, int b) =>
{
    return new { result = a + b };
})
.WithName("SumNumbers")
.WithTags("public");

// Multiply two numbers
app.MapGet("/multiply", (int a, int b) =>
{
    return new { result = a * b };
})
.WithName("MultiplyNumbers")
.WithTags("public");

// Get current UTC time
app.MapGet("/time", () =>
{
    return new { utc = DateTime.UtcNow };
})
.WithName("GetUtcTime")
.WithTags("public");

//
// ======================
// TOOL DISCOVERY ENDPOINT
// ======================
//

app.MapGet("/__tools", () =>
{
    return new object[]
    {
        // sum
        new
        {
            name = "sum",
            description = "Add two numbers",
            method = "GET",
            path = "/sum",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    a = new { type = "number" },
                    b = new { type = "number" }
                },
                required = new[] { "a", "b" }
            }
        },

        // multiply
        new
        {
            name = "multiply",
            description = "Multiply two numbers",
            method = "GET",
            path = "/multiply",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    a = new { type = "number" },
                    b = new { type = "number" }
                },
                required = new[] { "a", "b" }
            }
        },

        // time
        new
        {
            name = "time",
            description = "Get current UTC time",
            method = "GET",
            path = "/time",
            input_schema = new
            {
                type = "object",
                properties = new { }
            }
        }
    };
})
.WithName("ListTools")
.WithTags("internal");

app.Run();
