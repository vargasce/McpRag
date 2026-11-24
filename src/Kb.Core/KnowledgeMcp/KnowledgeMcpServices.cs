using Kb.Core.Abstractions;
using Kb.Core.Dtos;
using Kb.Core.Utils;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Kb.Core.KnowledgeMcp;

public sealed class KnowledgeMcpServices(
  IKnowledgeStore _store,
  IEmbeddingGenerator<string, Embedding<float>> _embeddings,
  IOptions<KbOptions> options
) : IKnowledgeSearchAsync
{
  public async Task<IReadOnlyList<DocumentResponseDto>> SearchAsync(
    string question,
    int top,
    string? category,
    CancellationToken ct = default
  )
  {
    top = Math.Clamp(top, 1, 20);

    var vector = await _embeddings.GenerateVectorAsync(
      options.Value.PrefijoConsulta + question,
      cancellationToken: ct
    );

    return await _store.SearchAsync(vector, top, category, ct);
  }
}
