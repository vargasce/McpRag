namespace Kb.Core.Dtos;

public sealed record DocumentResponseMcpDto(
  Guid DocumentId,
  string Title,
  string Section,
  string Category,
  string Text,
  double Score
);
