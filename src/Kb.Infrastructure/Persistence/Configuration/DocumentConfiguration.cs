using Kb.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
  public void Configure(EntityTypeBuilder<Document> entity)
  {
    entity.ToTable("documents");
    entity.HasKey(k => k.DocumentId);
    entity.Property(p => p.DocumentId).ValueGeneratedNever();

    entity.Property(d => d.RelativePath).HasMaxLength(400).IsRequired();
    entity.HasIndex(d => d.RelativePath).IsUnique();

    entity.Property(d => d.Category).HasMaxLength(100).IsRequired();
    entity.Property(d => d.Title).HasMaxLength(300).IsRequired();
    entity.Property(d => d.Hash).HasMaxLength(64);

    entity
      .Property(d => d.StatusDocument)
      .HasConversion<string>() // se guarda "Completado", no 2
      .HasMaxLength(20);

    entity.HasIndex(d => d.StatusDocument);
    entity.Property(d => d.LastError).HasMaxLength(2000);
  }
}
