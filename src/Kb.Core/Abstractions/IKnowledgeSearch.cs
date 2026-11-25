using Kb.Core.Dtos;

namespace Kb.Core.Abstractions;

public interface IKnowledgeSearchAsync
{
  Task<IReadOnlyList<DocumentResponseMcpDto>> SearchAsync(
    string question,
    int top,
    string? category,
    CancellationToken ct = default
  );

  Task<IReadOnlyList<DocumentDto>> ListDocumentAsync(CancellationToken ct = default);
  Task<DocumentDetailMcpDto?> GetDocumentByIdAsync(Guid documentId, CancellationToken ct = default);
  Task<string> GetIndexAsync(CancellationToken ct = default);
}
