using System.Runtime.CompilerServices;
using Kb.Core.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kb.Core.Ingestion;

public sealed class MarkdownSourceFile(
  IConfiguration configuration,
  ILogger<MarkdownSourceFile> _logger
)
{
  ///<summary>
  /// Read files of folder (docs/*.md) and return async enumerable
  ///</summary>
  ///<param name="CancellationToken">
  ///</param>
  public async IAsyncEnumerable<DocumentReadDto> ReadFilesAsync(
    [EnumeratorCancellation] CancellationToken ct = default
  )
  {
    var folder =
      configuration["ingest:folder"]
      ?? throw new InvalidOperationException("lack ingest configuration.");

    if (
      !Directory.Exists(folder)
      || Directory.GetFiles(folder, "*.md", SearchOption.AllDirectories).Length == 0
    )
    {
      _logger.LogError("Path or file not configure: {folder}", folder);
      throw new DirectoryNotFoundException($"Ingest folder not found: {folder}");
    }

    var files = Directory.GetFiles(folder, "*.md", SearchOption.AllDirectories);

    foreach (var path in files)
    {
      ct.ThrowIfCancellationRequested();

      var ruta = Path.GetRelativePath(folder, path).Replace('\\', '/');
      var category = ruta.Contains('/') ? ruta.Split('/')[0] : "general";
      var content = await File.ReadAllTextAsync(path, ct);

      yield return new DocumentReadDto(ruta, category, content);
    }
  }
}
