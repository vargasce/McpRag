using Kb.Core.Abstractions;
using Kb.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddKnowledgeIngest();

using var host = builder.Build();

await host.StartAsync();

await host.Services.InicializarBaseAsync();

await host.Services.GetRequiredService<IDocumentIngest>().ProcessIngestAsync();

await host.StopAsync();
