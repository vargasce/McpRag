using System.ComponentModel;
using Kb.Core.Abstractions;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Kb.McpServer.Resources;

[McpServerResourceType]
public class KnowledgeResources(IKnowledgeSearchAsync _knowledgeService)
{
  [McpServerResource(UriTemplate = "kb://indice", MimeType = "text/markdown", Name = "indice")]
  [Description(
    "Indice de la base de conocimiento: todos los documentos con su titulo, categorya y uri."
  )]
  public async Task<string> GetIndexAsync(CancellationToken ct = default)
  {
    var index = await _knowledgeService.GetIndexAsync(ct);
    return index;
  }

  [McpServerResource(
    UriTemplate = "kb://documento/{id}",
    MimeType = "text/markdown",
    Name = "documento"
  )]
  [Description("Contenido completo de un documento de la base de conocimineto.")]
  public async Task<string?> GetDocumentAsync(string id, CancellationToken ct = default)
  {
    if (!Guid.TryParse(id, out var documentId))
      throw new McpException($"{id} no es un valor de documento valido.");

    var document = await _knowledgeService.GetDocumentByIdAsync(documentId, ct);
    return document?.Content;
  }
}
