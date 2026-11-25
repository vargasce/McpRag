using Kb.Core.Abstractions;
using Kb.Core.Domain;
using Kb.Core.Dtos;
using Kb.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kb.Core.Ingestion;

public sealed class DocumentIngest(
  IChunkingManager _chunking,
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

      var hash = EncodingUtil.EncodeSha256(entry.Content);

      var document = await repo.GetByPathAsync(entry.RelativePath, ct);

      if (document is null)
      {
        document = Document.CreateDocument(entry.RelativePath, entry.Category);
        repo.AddDocument(document);
      }
      else if (!document.RequireProccess(hash))
      {
        _logger.SinCambios(entry.RelativePath);
        continue;
      }

      document.MarkProccess();
      await repo.SaveChangesAsync(ct);

      try
      {
        var chunks = _chunking
          .FragmentMarkDown(entry.Content)
          .Select(
            (parte, i) =>
              new Chunk(i, Path.GetFileName(entry.RelativePath), parte.section, parte.text)
          )
          .ToList();

        var chunkVectors = await _chunking.GetChunkVectors(chunks, document, ct);

        await _chunking.SaveChunckQdrant(chunkVectors, document, ct);

        document.MarkComplete(hash, entry.Title, entry.Content, chunks.Count);
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
        _logger.ErrorProcesando(ex, entry.RelativePath);
        document.MarkError(ex.Message);
      }

      await repo.SaveChangesAsync(ct);
    }

    _logger.LogInformation("Finish ingest.");
  }
}
