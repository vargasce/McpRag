using Kb.Core.Domain;
using Kb.Core.Dtos;

namespace Kb.Core.Abstractions;

public interface IChunkingManager
{
  List<(string section, string text)> FragmentMarkDown(string content);

  Task<IReadOnlyList<ReadOnlyMemory<float>>> GetEmbeddings(
    List<string> texts,
    CancellationToken ct = default
  );

  Task SaveChunckQdrant(
    List<ChunkVector> chunksVectors,
    Document document,
    CancellationToken ct = default
  );

  Task<List<ChunkVector>> GetChunkVectors(
    List<Chunk> chunks,
    Document document,
    CancellationToken ct = default
  );
}
