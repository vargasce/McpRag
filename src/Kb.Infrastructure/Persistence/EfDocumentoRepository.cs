using Kb.Core.Abstractions;
using Kb.Core.Domain;
using Kb.Core.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Kb.Infrastructure.Persistence;

internal sealed class EfDocumentoRepository(KbDbContext db) : IDocumentRepository
{
  public async Task<Document?> GetByPathAsync(string relativePhat, CancellationToken ct = default)
  {
    var document = await db.Documents.FirstOrDefaultAsync(
      x => x.RelativePath.Equals(relativePhat),
      ct
    );

    return document;
  }

  public async Task<IReadOnlyList<DocumentDto>> ListAsync(CancellationToken ct = default)
  {
    var listDocuments = await db
      .Documents.AsNoTracking()
      .OrderBy(x => x.CreateDate)
      .ThenBy(x => x.Title)
      .Select(d => new DocumentDto(
        d.RelativePath,
        d.Title,
        d.Category,
        d.StatusDocument.ToString(),
        d.CountChunks,
        d.LastDateProccess
      ))
      .ToListAsync(ct);

    return listDocuments;
  }

  public void AddDocument(Document document)
  {
    db.Documents.Add(document);
  }

  public Task SaveChangesAsync(CancellationToken ct = default)
  {
    return db.SaveChangesAsync(ct);
  }
}
