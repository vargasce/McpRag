var builder = DistributedApplication.CreateBuilder(args);

var qdrant = builder
  .AddQdrant("qdrant")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

var sql = builder
  .AddSqlServer("sqlserver")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

var kbdb = sql.AddDatabase("kbdb");

var ollama = builder
  .AddOllama("ollama")
  .WithDataVolume()
  .WithLifetime(ContainerLifetime.Persistent);

var embeddings = ollama.AddModel("embeddings", "nomic-embed-text");

var folder = Path.GetFullPath(Path.Combine("..", "..", "docs"));

builder
  .AddProject<Projects.Kb_McpServer>("mcp-server")
  .WithReference(qdrant)
  .WithReference(kbdb)
  .WithReference(embeddings)
  .WaitFor(qdrant)
  .WaitFor(kbdb)
  .WaitFor(embeddings);

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

builder.Build().Run();
