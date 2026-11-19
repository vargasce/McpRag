namespace Kb.Core.Dtos;

public sealed record DocumentDto(
  string RelativePath,
  string Title,
  string Category,
  string StatusDocument,
  int CountChunks,
  DateTimeOffset? LastDateProccess
);

public sealed record DocumentReadDto(string RelativePath, string Category, string Content);
