using Kb.Core.Abstractions;
using Kb.Core.Domain;
using Kb.Core.Dtos;
using Kb.Infrastructure.Persistence.Projections;
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

  public Task<Document?> GetDocumentDomainAsync(Guid documentId, CancellationToken ct = default)
  {
    return db
      .Documents.AsNoTracking()
      .Where(x => x.DocumentId == documentId)
      .FirstOrDefaultAsync(ct);
  }

  public Task<DocumentDetailMcpDto?> GetDocumentByIdAsync(Guid id, CancellationToken ct = default)
  {
    return db
      .Documents.AsNoTracking()
      .Where(d => d.DocumentId == id)
      .ProjectToDocumentDetailMcp()
      .FirstOrDefaultAsync(ct);
  }

  public async Task<IReadOnlyList<DocumentDto>> ListAsync(CancellationToken ct = default)
  {
    var listDocuments = await db
      .Documents.AsNoTracking()
      .OrderBy(x => x.CreateDate)
      .ThenBy(x => x.Title)
      .ProjectToDocumentDto()
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
