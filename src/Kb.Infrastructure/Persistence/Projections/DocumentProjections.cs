using System.Linq.Expressions;
using Kb.Core.Domain;
using Kb.Core.Dtos;

namespace Kb.Infrastructure.Persistence.Projections;

internal static class DocumentProjections
{
  public static readonly Expression<
    Func<Document, DocumentDetailMcpDto>
  > DocumentDetailMcpExpression = d => new DocumentDetailMcpDto(d.Title, d.Category, d.Content);

  public static IQueryable<DocumentDetailMcpDto> ProjectToDocumentDetailMcp(
    this IQueryable<Document> query
  ) => query.Select(DocumentDetailMcpExpression);

  public static readonly Expression<Func<Document, DocumentDto>> DocumentDtoExpression =
    d => new DocumentDto(
      d.DocumentId,
      d.RelativePath,
      d.Title,
      d.Category,
      d.StatusDocument.ToString(),
      d.CountChunks,
      d.LastDateProccess
    );

  public static IQueryable<DocumentDto> ProjectToDocumentDto(this IQueryable<Document> query) =>
    query.Select(DocumentDtoExpression);
}
