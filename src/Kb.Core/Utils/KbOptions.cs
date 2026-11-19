namespace Kb.Core.Utils;

public sealed class KbOptions
{
  public const string Seccion = "Kb";

  public string Coleccion { get; set; } = "kb_chunks";
  public int MaxCaracteresChunk { get; set; } = 1500;
  public string PrefijoDocumento { get; set; } = "search_document: ";
  public string PrefijoConsulta { get; set; } = "search_query: ";
}
