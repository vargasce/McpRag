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
  public async Task<IReadOnlyList<DocumentResponseDto>> SearchAsync(
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
}
