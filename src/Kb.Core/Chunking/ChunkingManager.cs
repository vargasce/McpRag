using System.Text;
using Kb.Core.Abstractions;
using Kb.Core.Domain;
using Kb.Core.Dtos;
using Kb.Core.Utils;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Kb.Core.Chunking;

public sealed class ChunkingManager(
  IEmbeddingGenerator<string, Embedding<float>> _embeddings,
  IKnowledgeStore _knowledgeStore,
  IOptions<KbOptions> options
) : IChunkingManager
{
  public List<(string section, string text)> FragmentMarkDown(string content)
  {
    var lineas = content.Split("\n");
    var titulo = lineas.FirstOrDefault(l => l.StartsWith("# "))?[2..].Trim();
    var section = titulo;
    var result = new List<(string section, string text)>();
    var buffer = new StringBuilder();

    void cerrar()
    {
      var texto = buffer.ToString().Trim();

      buffer.Clear();
      if (texto.Length == 0)
        return;

      var context = section == titulo ? titulo : $"{titulo} > {section}";
      var partes = new List<string> { texto };

      foreach (var p in partes)
      {
        result.Add((section!, $"{context} \n\n {p}"));
      }
    }

    foreach (var linea in lineas)
    {
      if (linea.StartsWith("## "))
      {
        cerrar();
        section = linea[3..].Trim();
      }
      else
      {
        buffer.AppendLine(linea);
      }
    }

    cerrar();

    return result;
  }

  public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GetEmbeddings(
    List<string> texts,
    CancellationToken ct = default
  )
  {
    var vector = await _embeddings.GenerateAsync(texts, cancellationToken: ct);

    return vector.Select(x => x.Vector).ToList();
  }

  public async Task SaveChunckQdrant(
    List<ChunkVector> chunksVectors,
    Document document,
    CancellationToken ct = default
  )
  {
    if (chunksVectors.Count == 0)
      return;

    await _knowledgeStore.ValidateColectionAsync(chunksVectors[0].Vector.Length, ct);
    await _knowledgeStore.UpdateChunksAsync(document.DocumentId, chunksVectors, ct);
  }

  public async Task<List<ChunkVector>> GetChunkVectors(
    List<Chunk> chunks,
    Document document,
    CancellationToken ct = default
  )
  {
    var chunkVectors = new List<ChunkVector>();

    const int lote = 16;
    for (int i = 0; i < chunks.Count; i += lote)
    {
      var grupo = chunks.Skip(i).Take(lote).ToList();
      var texts = grupo.Select(c => $"{options.Value.PrefijoDocumento}{c.Text}").ToList();
      var vectores = await GetEmbeddings(texts, ct);

      for (int j = 0; j < grupo.Count; j++)
      {
        chunkVectors.Add(new ChunkVector(document.DocumentId, chunks[i + j], vectores[j]));
      }
    }

    return chunkVectors;
  }
}
