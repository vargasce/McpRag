using Kb.Core.Dtos;

namespace Kb.Core.Abstractions;

public interface IKnowledgeSearchAsync
{
  Task<IReadOnlyList<DocumentResponseDto>> SearchAsync(
    string question,
    int top,
    string? category,
    CancellationToken ct = default
  );
}
