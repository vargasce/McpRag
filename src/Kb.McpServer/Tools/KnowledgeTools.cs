using System.ComponentModel;
using Kb.Core.Abstractions;
using Kb.Core.Dtos;
using ModelContextProtocol.Server;

namespace Kb.McpServer.Tools;

[McpServerToolType]
public class KnowledgeTools(
  IKnowledgeSearchAsync _knowledgeService,
  ILogger<KnowledgeTools> _logger
)
{
  [McpServerTool(Name = "busqueda-de-conocimiento", ReadOnly = true)]
  [Description(
    """
        Busca en la base de conocimiento de la plataforma (onboarding, cuenta comitente,
        descuento de facturas, e-cheqs y pagarés, inversiones, liquidaciones) los fragmentos
        más relevantes para una pregunta. Usala antes de responder cualquier consulta sobre
        cómo funciona la plataforma y citá el título del documento de origen.
        Si los scores son bajos (menores a 0.5), avisá que la información puede no estar en la base.
      """
  )]
  public async Task<IReadOnlyList<DocumentResponseMcpDto>> SearchAsync(
    [Description("Pregunta o tema a buscar, en lenguaje natural")] string question,
    [Description("Opcional: filtra por categoría (onboarding, alyc, descuento)")]
      string? category = null,
    [Description("Cantidad de fragmentos a devolver, entre 1 y 20")] int top = 5,
    CancellationToken ct = default
  )
  {
    try
    {
      _logger.LogInformation("Init request mcp");
      var response = await _knowledgeService.SearchAsync(question, top, category, ct);

      _logger.LogInformation(
        "Búsqueda con categoría {Categoria}: {Cantidad} resultados, mejor score {MejorScore}",
        category ?? "(todas)",
        response?.Count,
        response?.FirstOrDefault()?.Score
      );

      return response ?? [];
    }
    finally
    {
      _logger.LogInformation("Finish request mcp");
    }
  }

  [McpServerTool(Name = "lista-de-documentos", ReadOnly = true)]
  [Description(
    """
      Lista los documentos disponibles en la base de conocimiento, con su id, título y categoría.
      Usala para saber qué temas cubre la base, para conocer las categorías válidas antes de
      filtrar una búsqueda, o para encontrar el id de un documento y leerlo completo con
      la tool obtener-documento.
      """
  )]
  public async Task<IReadOnlyList<DocumentDto>> ListDocumentsAsync(CancellationToken ct = default)
  {
    try
    {
      _logger.LogInformation("Init request mcp");
      var documents = await _knowledgeService.ListDocumentAsync(ct);
      return documents;
    }
    finally
    {
      _logger.LogInformation("Finish request mcp");
    }
  }

  [McpServerTool(Name = "obtener-documento", ReadOnly = true)]
  [Description(
    """
      Devuelve el contenido completo (en Markdown) de un documento de la base de conocimiento,
      junto con su título y categoría. Usala cuando los fragmentos de buscar-conocimiento
      no alcanzan para responder, o cuando un fragmento hace referencia a otra guía
      (por ejemplo, "ver Descuento — Negociación y ofertas"). El id del documento viene
      en los resultados de buscar-conocimiento y de listar-documentos.
      """
  )]
  public async Task<DocumentDetailMcpDto?> GetDocumentAsync(
    [Description(
      "Id del documento, tal como aparece en documentId en los resultados de buscar-conocimiento o listar-documentos"
    )]
      Guid documentId,
    CancellationToken ct = default
  )
  {
    try
    {
      _logger.LogInformation("Init request mcp");
      var document = await _knowledgeService.GetDocumentByIdAsync(documentId, ct);
      return document;
    }
    finally
    {
      _logger.LogInformation("Finish request mcp");
    }
  }
}
