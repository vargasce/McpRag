using Kb.Core.Abstractions;
using Kb.Core.Chunking;
using Kb.Core.Ingestion;
using Kb.Core.KnowledgeMcp;
using Kb.Core.Utils;
using Kb.Infrastructure.Persistence;
using Kb.Infrastructure.VectorStore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kb.Infrastructure;

public static class DependencyInjection
{
  ///<summary>
  /// Add services base of App KnowledgeBase
  ///</summary>
  ///<param name="builder"> this IHostApplicationBuilder </param>
  public static IHostApplicationBuilder AddKnowledgeBase(this IHostApplicationBuilder builder)
  {
    builder.AddOllamaApiClient("embeddings").AddEmbeddingGenerator();
    builder.AddSqlServerDbContext<KbDbContext>("kbdb");
    builder.AddQdrantClient("qdrant");

    builder.Services.AddSingleton<IKnowledgeStore, KnowledgeStore>();
    builder.Services.AddScoped<IDocumentRepository, EfDocumentoRepository>();

    builder
      .Services.AddOptions<KbOptions>()
      .Bind(builder.Configuration.GetSection(KbOptions.Seccion))
      .ValidateOnStart();

    return builder;
  }

  ///<summary>
  /// Add service by Ingest proyect.
  ///</summary>
  ///<param name="builder"> this IHostApplicationBuilder </param>
  public static IHostApplicationBuilder AddKnowledgeIngest(this IHostApplicationBuilder builder)
  {
    builder.AddKnowledgeBase();
    builder.Services.AddSingleton<IChunkingManager, ChunkingManager>();
    builder.Services.AddSingleton<IDocumentIngest, DocumentIngest>();
    builder.Services.AddSingleton<MarkdownSourceFile>();

    return builder;
  }

  ///<summary>
  /// Add service by MPC proyect.
  ///</summary>
  ///<param name="builder"> this IHostApplicationBuilder </param>
  public static IHostApplicationBuilder AddKnowledgeMcp(this IHostApplicationBuilder builder)
  {
    builder.AddKnowledgeBase();
    builder.Services.AddScoped<IKnowledgeSearchAsync, KnowledgeMcpServices>();
    return builder;
  }

  public static async Task InicializarBaseAsync(this IServiceProvider services)
  {
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KbDbContext>();
    await db.Database.EnsureCreatedAsync();
  }
}
