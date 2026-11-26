namespace Kb.Core.Domain;

public sealed class Document
{
  public Guid DocumentId { get; private set; }
  public string RelativePath { get; private set; } = null!;
  public string Category { get; private set; } = null!;
  public string Title { get; private set; } = null!;
  public string? Hash { get; private set; }
  public StatusDocumentEnum StatusDocument { get; private set; }
  public int CountChunks { get; private set; }
  public int Attempts { get; private set; }
  public string? LastError { get; private set; }
  public string? Content { get; private set; }
  public DateTimeOffset CreateDate { get; private set; }
  public DateTimeOffset? LastDateProccess { get; private set; }

  public Document() { }

  public static Document CreateDocument(
    string relativePath,
    string category,
    string title,
    string content
  ) =>
    new()
    {
      Content = content,
      DocumentId = Guid.NewGuid(),
      RelativePath = relativePath,
      Category = category,
      Title = title,
      StatusDocument = StatusDocumentEnum.Pending,
      CreateDate = DateTimeOffset.Now,
    };

  public bool RequireProccess(string hash) =>
    !(hash == Hash && StatusDocument == StatusDocumentEnum.Complete);

  public void MarkProccess()
  {
    StatusDocument = StatusDocumentEnum.Proccess;
    CountChunks++;
    LastError = null;
  }

  public void MarkComplete(string hash, string? content, int countChunks)
  {
    Hash = hash;
    Content = content;
    CountChunks = countChunks;
    StatusDocument = StatusDocumentEnum.Complete;
    LastDateProccess = DateTimeOffset.Now;
  }

  public void MarkError(string error)
  {
    LastError = error.Length > 2000 ? error[..2000] : error;
    StatusDocument = StatusDocumentEnum.Error;
    LastDateProccess = DateTimeOffset.Now;
  }

  public void UpdateContent(string content, string category, string title)
  {
    Content = content;
    Category = category;
    Title = title;
  }
}
