namespace Kb.Core.Abstractions;

public interface IDocumentProcessor
{
  Task ProcessAsync(Guid documentId, CancellationToken ct = default);
}
