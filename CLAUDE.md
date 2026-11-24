# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Qué es

Servidor MCP en .NET 10 que expone una base de conocimiento corporativa mediante RAG. Stack: Aspire · Model Context Protocol · Qdrant (vectores) · SQL Server (estado de documentos) · Ollama con `nomic-embed-text` (embeddings) · EF Core. El plan completo y el roadmap por fases están en `../../init-proyect/README.md` (fuera de este repo git).

## Comandos

```bash
dotnet build                          # compila la solución (KnowledgeBase.slnx)
dotnet run --project src/Kb.AppHost   # levanta todo con Aspire (requiere Docker)
```

- Aspire levanta los contenedores `qdrant`, `sqlserver` (db `kbdb`) y `ollama` (modelo `embeddings` = nomic-embed-text, ~270 MB la primera vez) y el `mcp-server`.
- `ingest` está con `WithExplicitStart()`: se dispara a mano desde el dashboard de Aspire. Es un proceso one-shot que ingesta la carpeta `docs/` del repo (el AppHost la pasa vía env `ingest__folder`).
- El MCP server expone Streamable HTTP en `/mcp` (puerto local 5159 según launchSettings). Probar con `npx @modelcontextprotocol/inspector` o `claude mcp add --transport http kb-fintech http://localhost:5159/mcp`.
- No hay tests todavía (fase 4 del roadmap).

## Arquitectura

Regla de dependencias: `Kb.Core` no conoce a nadie → `Kb.Infrastructure` conoce a `Core` → los ejecutables (`Kb.McpServer`, `Kb.Ingest`) usan `Infrastructure` + `ServiceDefaults` → `Kb.AppHost` solo referencia lo que orquesta.

- **Kb.Core**: dominio (`Document` con máquina de estados Pending/Proccess/Complete/Error), abstracciones (`IKnowledgeStore`, `IDocumentRepository`, `IChunkingManager`, `IDocumentIngest`), chunking e ingesta. Sin dependencias de infraestructura.
- **Kb.Infrastructure**: `DependencyInjection.AddKnowledgeBase()` es el punto de entrada que registra todo (clientes Aspire + servicios). `KbDbContext` usa `EnsureCreated` (sin migraciones por ahora). `KnowledgeStore` maneja la colección Qdrant.
- **Kb.McpServer**: `WithToolsFromAssembly()` descubre las tools por atributos — las tools MCP aún no están implementadas.
- **Kb.Ingest**: console app que corre `InicializarBaseAsync()` (EnsureCreated) y luego `ProcessIngestAsync()`.

### Nombres de recursos Aspire = claves de connection string

`"qdrant"`, `"kbdb"` y `"embeddings"` se declaran en `AppHost.cs` y deben coincidir con los `AddQdrantClient("qdrant")`, `AddSqlServerDbContext<KbDbContext>("kbdb")` y `AddOllamaApiClient("embeddings")` de `DependencyInjection.cs`. Cambiar un nombre rompe la inyección.

### Pipeline de ingesta (Kb.Core/Ingestion + Chunking, Kb.Infrastructure/VectorStore)

1. `MarkdownSourceFile` recorre `docs/**/*.md`; la primera subcarpeta es la categoría (si no hay, `"general"`).
2. `DocumentIngest` calcula SHA-256 del contenido; si el hash no cambió y el documento está `Complete`, lo saltea (idempotencia). El estado se persiste en SQL Server antes y después de procesar.
3. `ChunkingManager.FragmentMarkDown`: el primer `#` es el título, cada `##` abre una sección; a cada chunk se le antepone el contexto `"Titulo > Seccion"`.
4. Embeddings en lotes de 16, anteponiendo el prefijo `search_document: ` (`PrefijoDocumento`). Los prefijos son propios de nomic-embed-text (distingue documento vs consulta: `search_query: ` para buscar).
5. `KnowledgeStore`: colección Qdrant `kb_chunks` (coseno, dimensión tomada del primer embedding, índices de payload en `documentoId` y `categoria`). El update borra los puntos del documento por `documentoId` y reinserta.

### Configuración

`KbOptions` (sección `"Kb"`): colección, tamaño máximo de chunk y prefijos de embedding. Tiene defaults en código; se puede sobrescribir en appsettings de cada ejecutable.

## Estado actual / pendientes conocidos

- `KnowledgeStore.BuscarAsync` no está implementado (lanza excepción): la búsqueda semántica y las tools MCP (`buscar_conocimiento`) son el siguiente paso de la fase 1.
- `FragmentMarkDown` todavía no parte secciones largas por `MaxCaracteresChunk` (la lista `partes` siempre tiene un solo elemento).

## Convenciones

- Comentarios y nombres de dominio en español; código en PascalCase/camelCase según convención C#.
- Commits estilo Conventional Commits (`feat|fix|chore|refactor|docs|test`).
