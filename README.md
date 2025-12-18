MCP Gateway with Dapr Auto-Discovery (.NET)
Overview

This project demonstrates a Model Context Protocol (MCP) Gateway built with .NET and Dapr that automatically exposes microservices as AI-callable tools.

Instead of manually registering tools for every service, this gateway uses Dapr service invocation + tool discovery to make new services appear automatically in AI clients like Claude Desktop.

Goal:

Add a new microservice → it becomes an AI tool with zero gateway changes.

Why This Exists

Traditional AI tool integration is hard to scale:

Every new tool requires manual registration

Gateways become bottlenecks

AI clients must be reconfigured often

This project solves that by:

Using Dapr for service discovery & invocation

Using a standard /__tools endpoint per service

Translating services → MCP tools dynamically


+--------------------+
|   Claude Desktop   |
|   (MCP Client)     |
+----------+---------+
           |
           | JSON-RPC (stdio / HTTP)
           v
+-----------------------------+
|        MCP Gateway          |
|  - tools/list               |
|  - tools/call               |
|  - Auto-discovery logic     |
+--------------+--------------+
               |
               | Dapr Service Invocation
               v
+-----------------------------+
|        Dapr Sidecar         |
|  (Service Discovery Layer) |
+--------------+--------------+
               |
               v
+-----------------------------+
|        MathService          |
|  /sum                       |
|  /time                      |
|  /__tools                   |
+-----------------------------+



2️⃣ Dapr

Dapr provides:

Service discovery

Sidecar networking

Standardized service invocation

MathService is invoked through Dapr:



http://localhost:3500/v1.0/invoke/math-service/method/sum
No hardcoded IPs or ports are required.



3️⃣ MCP Gateway (.NET)

The gateway acts as a bridge between AI clients and Dapr services.

It implements:

tools/list

Calls /__tools on all registered services

Merges results

Returns MCP-compatible tool definitions

tools/call

Receives tool name + arguments

Routes the call via Dapr to the correct service

Returns the result to the AI client

This makes the gateway generic and scalable.

How Auto-Discovery Works

Gateway knows which services exist (initially static, later dynamic)

Gateway calls:

/v1.0/invoke/<app-id>/method/__tools


Services describe themselves

Gateway exposes them as MCP tools

Result

✅ Add a new service
✅ Implement /__tools
✅ Run with Dapr
➡ Tool appears automatically in Claude

Running Locally
Prerequisites

.NET 8+

Docker

Dapr CLI

WSL (recommended on Windows)

Start Dapr
dapr init

Run MathService
cd MathService
dapr run \
  --app-id math-service \
  --app-port 5000 \
  --dapr-http-port 3500 \
  -- \
  dotnet run --no-launch-profile
curl "http://localhost:3500/v1.0/invoke/math-service/method/sum?a=3&b=5"


Run MCP Gateway
cd McpGateway
dotnet run

Test Manually

List tools:

curl -X POST http://localhost:5258 \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'


Call a tool:

curl -X POST http://localhost:5258 \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc":"2.0",
    "id":2,
    "method":"tools/call",
    "params":{
      "name":"sum",
      "arguments":{"a":3,"b":5}
    }
  }'
