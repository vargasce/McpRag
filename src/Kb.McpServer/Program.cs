using Kb.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKnowledgeMcp();

builder
  .Services.AddMcpServer()
  .WithHttpTransport()
  .WithToolsFromAssembly()
  .WithResourcesFromAssembly()
  .WithPromptsFromAssembly();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp("mcp");

app.Run();
