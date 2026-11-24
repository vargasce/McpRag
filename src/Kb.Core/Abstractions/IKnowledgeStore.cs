using Kb.Core.Domain;
using Kb.Core.Dtos;

namespace Kb.Core.Abstractions;

public interface IKnowledgeStore
{
  Task ValidateColectionAsync(int dimension, CancellationToken ct = default);

  Task UpdateChunksAsync(
    Document document,
    IReadOnlyList<ChunkVector> chunks,
    CancellationToken ct = default
  );

  Task<IReadOnlyList<DocumentResponseDto>> SearchAsync(
    ReadOnlyMemory<float> vector,
    int top,
    string? categoria,
    CancellationToken ct = default
  );
}
