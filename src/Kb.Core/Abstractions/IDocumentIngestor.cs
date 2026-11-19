namespace Kb.Core.Abstractions;

public interface IDocumentIngest
{
  Task ProcessIngestAsync(CancellationToken ct = default);
}
