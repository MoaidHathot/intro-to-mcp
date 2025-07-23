using Dumpify;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
	builder
	.AddConsole()
	.SetMinimumLevel(LogLevel.Information);
});

var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
	Name = "test-sample-client",
	Command = "dotnet",
	Arguments = ["run", "--project", "./sample-mcp-server"],
});

var client = await McpClientFactory.CreateAsync(clientTransport, loggerFactory: loggerFactory);

(await client.ListToolsAsync()).Select(t => (name: t.Name, description: t.Description)).Dump();

var result = await client.CallToolAsync(
	"get_recipe",
	new Dictionary<string, object?>() { 
		{ "recipeName", "falafel" }
	},
	cancellationToken:CancellationToken.None);

result.Content.Dump();
