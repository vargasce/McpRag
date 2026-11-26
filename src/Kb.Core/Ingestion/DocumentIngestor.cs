using Kb.Core.Abstractions;
using Kb.Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kb.Core.Ingestion;

public sealed class DocumentIngest(
  MarkdownSourceFile _source,
  IServiceScopeFactory _factory,
  ILogger<DocumentIngest> _logger
) : IDocumentIngest
{
  public async Task ProcessIngestAsync(CancellationToken ct = default)
  {
    _logger.LogInformation("Init ingest.");

    await foreach (var entry in _source.ReadFilesAsync(ct))
    {
      await using var scope = _factory.CreateAsyncScope();
      var repo = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
      var documentProcesor = scope.ServiceProvider.GetRequiredService<IDocumentProcessor>();

      var document = await repo.GetByPathAsync(entry.RelativePath, ct);

      if (document is null)
      {
        document = Document.CreateDocument(
          entry.RelativePath,
          entry.Category,
          entry.Title,
          entry.Content
        );

        repo.AddDocument(document);
      }
      else
      {
        document.UpdateContent(entry.Content, entry.Category, entry.Title);
      }

      await repo.SaveChangesAsync(ct);

      await documentProcesor.ProcessAsync(document.DocumentId, ct);
    }

    _logger.LogInformation("Finish ingest.");
  }
}
