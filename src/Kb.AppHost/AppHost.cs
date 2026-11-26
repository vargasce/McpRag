var builder = DistributedApplication.CreateBuilder(args);

// Qdrant
var qdrant = builder
  .AddQdrant("qdrant")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

// Sqlserver
var sql = builder
  .AddSqlServer("sqlserver")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

var kbdb = sql.AddDatabase("kbdb");

// Ollama
var ollama = builder
  .AddOllama("ollama")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

var embeddings = ollama.AddModel("embeddings", "bge-m3");

// folder
var folder = Path.GetFullPath(Path.Combine("..", "..", "docs"));

// service bus
var serviceBus = builder
  .AddAzureServiceBus("messaging")
  .RunAsEmulator(emulador => emulador.WithLifetime(ContainerLifetime.Persistent));

var ingestQueue = serviceBus
  .AddServiceBusQueue("ingest")
  .WithProperties(queue =>
  {
    queue.MaxDeliveryCount = 5;
    queue.LockDuration = TimeSpan.FromMinutes(2);
  });

// MCP
builder
  .AddProject<Projects.Kb_McpServer>("mcp-server")
  .WithReference(qdrant)
  .WithReference(kbdb)
  .WithReference(embeddings)
  .WaitFor(qdrant)
  .WaitFor(kbdb)
  .WaitFor(embeddings);

// INGEST
builder
  .AddProject<Projects.Kb_Ingest>("ingest")
  .WithReference(qdrant)
  .WithReference(kbdb)
  .WithReference(embeddings)
  .WaitFor(qdrant)
  .WaitFor(kbdb)
  .WaitFor(embeddings)
  .WithEnvironment("ingest__folder", folder)
  .WithExplicitStart(); // se dispara a mano desde el dashboard

// INGEST API
builder
  .AddProject<Projects.Kb_IngestionApi>("ingestion-api", launchProfileName: "http")
  .WithReference(kbdb)
  .WithReference(serviceBus)
  .WaitFor(kbdb)
  .WaitFor(serviceBus);

builder.Build().Run();
