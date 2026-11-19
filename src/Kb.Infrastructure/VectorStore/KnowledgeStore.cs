using Kb.Core.Abstractions;
using Kb.Core.Dtos;
using Kb.Core.Utils;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace Kb.Infrastructure.VectorStore;

public sealed class KnowledgeStore(QdrantClient qdrant, IOptions<KbOptions> options)
  : IKnowledgeStore
{
  private readonly string _collection = options.Value.Coleccion;

  public async Task ValidateColectionAsync(int dimension, CancellationToken ct = default)
  {
    if (await qdrant.CollectionExistsAsync(_collection, ct))
      return;

    await qdrant.CreateCollectionAsync(
      _collection,
      new VectorParams { Size = (ulong)dimension, Distance = Distance.Cosine },
      cancellationToken: ct
    );

    // Índices para filtrar rápido por estos campos del payload
    await qdrant.CreatePayloadIndexAsync(
      _collection,
      "documentoId",
      PayloadSchemaType.Keyword,
      cancellationToken: ct
    );
    await qdrant.CreatePayloadIndexAsync(
      _collection,
      "categoria",
      PayloadSchemaType.Keyword,
      cancellationToken: ct
    );
  }

  public async Task UpdateChunksAsync(
    Guid documentoId,
    IReadOnlyList<ChunkVector> chunks,
    CancellationToken ct = default
  )
  {
    await DeleteByDocumentIdAsync(documentoId, ct);

    //Armar los puntos nuevos
    var points = chunks
      .Select(c => new PointStruct
      {
        Id = Guid.NewGuid(),
        Vectors = c.Vector.ToArray(),
        Payload =
        {
          ["documentoId"] = documentoId.ToString(),
          ["titulo"] = c.Chunking.File,
          ["categoria"] = c.Chunking.File,
          ["seccion"] = c.Chunking.Section,
          ["indice"] = c.Chunking.Index,
          ["texto"] = c.Chunking.Text,
        },
      })
      .ToList();

    await qdrant.UpsertAsync(_collection, points, cancellationToken: ct);
  }

  public Task<IReadOnlyList<DocumentDto>> BuscarAsync(
    ReadOnlyMemory<float> vector,
    int top,
    string? categoria,
    CancellationToken ct = default
  )
  {
    throw new Exception();
  }

  private async Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
  {
    await qdrant.DeleteAsync(
      _collection,
      new Filter { Must = { MatchKeyword("documentoId", documentId.ToString()) } },
      cancellationToken: ct
    );
  }
}
