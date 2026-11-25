using System.Text;
using Kb.Core.Abstractions;
using Kb.Core.Dtos;
using Kb.Core.Utils;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Kb.Core.KnowledgeMcp;

public sealed class KnowledgeMcpServices(
  IDocumentRepository _repo,
  IKnowledgeStore _store,
  IEmbeddingGenerator<string, Embedding<float>> _embeddings,
  IOptions<KbOptions> options
) : IKnowledgeSearchAsync
{
  public async Task<IReadOnlyList<DocumentResponseMcpDto>> SearchAsync(
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

  public async Task<IReadOnlyList<DocumentDto>> ListDocumentAsync(CancellationToken ct = default)
  {
    var documents = await _repo.ListAsync(ct);
    return documents;
  }

  public async Task<DocumentDetailMcpDto?> GetDocumentByIdAsync(
    Guid documentId,
    CancellationToken ct = default
  )
  {
    var document = await _repo.GetDocumentByIdAsync(documentId, ct);
    return document;
  }

  public async Task<string> GetIndexAsync(CancellationToken ct = default)
  {
    var documents = await _repo.ListAsync(ct);
    var sb = new StringBuilder("# indice de la base de conocimiento.");

    foreach (var category in documents.GroupBy(x => x.Category).OrderBy(o => o.Key))
    {
      sb.Append($"\n ## Categoria {category.Key} \n\n");
      foreach (var d in category.OrderBy(o => o.Title))
      {
        sb.Append($"- {d.Title} - kb://documento/{d.DocumentId} \n");
      }
    }

    return sb.ToString();
  }
}
