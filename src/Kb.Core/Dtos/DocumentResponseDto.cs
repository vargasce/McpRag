namespace Kb.Core.Dtos;

public sealed record DocumentResponseDto(
  Guid DocumentId,
  string Title,
  string Section,
  string Category,
  string Text,
  double Score
);
