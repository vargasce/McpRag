using Kb.Core.Dtos;

namespace Kb.Core.Abstractions;

public interface IKnowledgeStore
{
  Task ValidateColectionAsync(int dimension, CancellationToken ct = default);

  Task UpdateChunksAsync(
    Guid documentoId,
    IReadOnlyList<ChunkVector> chunks,
    CancellationToken ct = default
  );

  Task<IReadOnlyList<DocumentDto>> BuscarAsync(
    ReadOnlyMemory<float> vector,
    int top,
    string? categoria,
    CancellationToken ct = default
  );
}
