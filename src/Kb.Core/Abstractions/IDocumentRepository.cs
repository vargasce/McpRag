using Kb.Core.Domain;
using Kb.Core.Dtos;

namespace Kb.Core.Abstractions;

public interface IDocumentRepository
{
  Task<Document?> GetByPathAsync(string relativePhat, CancellationToken ct = default);
  Task<IReadOnlyList<DocumentDto>> ListAsync(CancellationToken ct = default);
  Task<DocumentDetailMcpDto?> GetDocumentByIdAsync(Guid id, CancellationToken ct = default);
  void AddDocument(Document document);
  Task SaveChangesAsync(CancellationToken ct = default);
}
