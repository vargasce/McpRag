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
    Core.Domain.Document document,
    IReadOnlyList<ChunkVector> chunks,
    CancellationToken ct = default
  )
  {
    await DeleteByDocumentIdAsync(document.DocumentId, ct);

    //Armar los puntos nuevos
    var points = chunks
      .Select(c => new PointStruct
      {
        Id = Guid.NewGuid(),
        Vectors = c.Vector.ToArray(),
        Payload =
        {
          ["documentoId"] = document.DocumentId.ToString(),
          ["titulo"] = document.Title,
          ["categoria"] = document.Category,
          ["seccion"] = c.Chunking.Section,
          ["indice"] = c.Chunking.Index,
          ["texto"] = c.Chunking.Text,
        },
      })
      .ToList();

    await qdrant.UpsertAsync(_collection, points, cancellationToken: ct);
  }

  public async Task<IReadOnlyList<DocumentResponseDto>> SearchAsync(
    ReadOnlyMemory<float> vector,
    int top,
    string? categoria,
    CancellationToken ct = default
  )
  {
    if (!await qdrant.CollectionExistsAsync(_collection, ct))
      return [];

    Filter? filter = string.IsNullOrEmpty(categoria)
      ? null
      : new Filter { Must = { MatchKeyword("category", categoria.Trim().ToLower()) } };

    var points = await qdrant.QueryAsync(
      _collection,
      query: vector.ToArray(),
      filter: filter,
      limit: (ulong)top,
      scoreThreshold: options.Value.ScoreMinimo,
      payloadSelector: true,
      cancellationToken: ct
    );

    return points
      .Select(p => new DocumentResponseDto(
        DocumentId: Guid.Parse(GetString(p.Payload, "documentoId")),
        Title: GetString(p.Payload, "titulo"),
        Section: GetString(p.Payload, "seccion"),
        Category: GetString(p.Payload, "categoria"),
        Text: GetString(p.Payload, "texto"),
        Score: Math.Round(p.Score, 3)
      ))
      .ToList();
  }

  private static string GetString(IDictionary<string, Value> payload, string key) =>
    payload.TryGetValue(key, out var value) ? value.StringValue : "";

  private async Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
  {
    await qdrant.DeleteAsync(
      _collection,
      new Filter { Must = { MatchKeyword("documentId", documentId.ToString()) } },
      cancellationToken: ct
    );
  }
}
