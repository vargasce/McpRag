using Microsoft.Extensions.Logging;

namespace Kb.Core.Utils;

internal static partial class IngestLog
{
  [LoggerMessage(Level = LogLevel.Information, Message = "{Ruta}: sin cambios, se omite")]
  public static partial void SinCambios(this ILogger logger, string ruta);

  [LoggerMessage(Level = LogLevel.Information, Message = "{Ruta}: procesado en {Chunks} chunks")]
  public static partial void Procesado(this ILogger logger, string ruta, int chunks);

  [LoggerMessage(Level = LogLevel.Error, Message = "{Ruta}: error al procesar")]
  public static partial void ErrorProcesando(this ILogger logger, Exception ex, string ruta);
}
