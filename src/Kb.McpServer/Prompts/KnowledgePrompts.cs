using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Kb.McpServer.Prompts;

[McpServerPromptType]
public sealed class KnowledgePrompts
{
  [McpServerPrompt(Name = "responder_con_fuentes")]
  [Description(
    "Responde una consulta sobre la plataforma usando solo la base de conocimiento, citando las fuentes"
  )]
  public static string ResponderConFuentes(
    [Description("La consulta a responder")] string pregunta
  ) =>
    $"""
      Respondé la siguiente consulta sobre la plataforma Finvoi.

      Cómo trabajar:
      1. Buscá con la tool busqueda-de-conocimiento antes de responder. Si la consulta
         abarca más de un tema, hacé una búsqueda por cada uno.
      2. Si un fragmento remite a otra guía ("ver ..."), traela con obtener-documento.
      3. Respondé solo con lo que dicen los documentos. No completes con conocimiento general.

      Formato de la respuesta:
      - Si es un procedimiento, en pasos numerados.
      - Al final, una línea "Fuentes:" con el título y la sección de cada documento usado.

      Si la búsqueda no devuelve resultados, o no responden la consulta, decí claramente
      que esa información no está en la base de conocimiento.

      Consulta: {pregunta}
      """;
}
