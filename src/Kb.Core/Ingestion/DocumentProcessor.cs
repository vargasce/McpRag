using Kb.Core.Abstractions;
using Kb.Core.Domain;
using Kb.Core.Dtos;
using Kb.Core.Utils;
using Microsoft.Extensions.Logging;

namespace Kb.Core.Ingestion;

public sealed class DocumentProcessor(
  IChunkingManager _chunking,
  ILogger<DocumentProcessor> _logger,
  IDocumentRepository repo
) : IDocumentProcessor
{
  public async Task ProcessAsync(Guid documentId, CancellationToken ct = default)
  {
    var document = await repo.GetDocumentDomainAsync(documentId, ct);

    if (document is null || document.Content is null)
      return;

    var hash = EncodingUtil.EncodeSha256(document.Content);

    if (!document.RequireProccess(hash))
      return;

    document.MarkProccess();

    await repo.SaveChangesAsync(ct);

    await CreateAndSaveChunking(document, hash, ct);
  }

  private async Task CreateAndSaveChunking(
    Document document,
    string hash,
    CancellationToken ct = default
  )
  {
    try
    {
      var chunks = _chunking
        .FragmentMarkDown(document.Content)
        .Select(
          (parte, i) =>
            new Chunk(i, Path.GetFileName(document.RelativePath), parte.section, parte.text)
        )
        .ToList();

      var chunkVectors = await _chunking.GetChunkVectors(chunks, document, ct);

      await _chunking.SaveChunckQdrant(chunkVectors, document, ct);

      document.MarkComplete(hash, document.Content, chunks.Count);
    }
    catch (Exception ex) when (!ct.IsCancellationRequested)
    {
      _logger.ErrorProcesando(ex, document.RelativePath);
      document.MarkError(ex.Message);
    }

    await repo.SaveChangesAsync(ct);
  }
}
