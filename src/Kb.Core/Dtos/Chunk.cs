namespace Kb.Core.Dtos;

public record Chunk(int Index, string File, string Section, string Text);

public record ChunkVector(Guid DocumentId, Chunk Chunking, ReadOnlyMemory<float> Vector);
