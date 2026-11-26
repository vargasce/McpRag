# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Qué es

Servidor MCP en .NET 10 que expone una base de conocimiento corporativa mediante RAG. Stack: Aspire · Model Context Protocol · Qdrant (vectores) · SQL Server (estado y contenido de documentos) · Ollama con `bge-m3` (embeddings multilingües) · EF Core. El plan completo y el roadmap por fases están en `../../init-proyect/README.md` (fuera de este repo git).

## Comandos

```bash
dotnet build                          # compila la solución (KnowledgeBase.slnx)
dotnet run --project src/Kb.AppHost   # levanta todo con Aspire (requiere Docker)
```

- Aspire levanta los contenedores `qdrant`, `sqlserver` (db `kbdb`) y `ollama` (modelo `embeddings` = `bge-m3`) y el `mcp-server`. Los contenedores son persistentes (`ContainerLifetime.Persistent` + volúmenes): sobreviven al cierre del AppHost.
- `ingest` está con `WithExplicitStart()`: se dispara a mano desde el dashboard de Aspire. Es un proceso one-shot que ingesta la carpeta `docs/` del repo (el AppHost la pasa vía env `ingest__folder`).
- El MCP server expone Streamable HTTP en `/mcp`, con puerto fijo del perfil `http` de `launchSettings.json` (el AppHost usa `launchProfileName: "http"`). Probar con `npx @modelcontextprotocol/inspector` o desde Claude Code, que lo tiene configurado en `.mcp.json` como `kb-base`.
- No hay tests todavía (fase 4 del roadmap).

## Arquitectura

Regla de dependencias: `Kb.Core` no conoce a nadie → `Kb.Infrastructure` conoce a `Core` → los ejecutables (`Kb.McpServer`, `Kb.Ingest`) usan `Infrastructure` + `ServiceDefaults` → `Kb.AppHost` solo referencia lo que orquesta.

- **Kb.Core**: dominio (`Document` con máquina de estados Pending/Proccess/Complete/Error), abstracciones (`IKnowledgeStore`, `IDocumentRepository`, `IChunkingManager`, `IDocumentIngest`, servicio de búsqueda), DTOs, chunking e ingesta. Sin dependencias de infraestructura.
- **Kb.Infrastructure**: punto de entrada de DI en `DependencyInjection`:
  - `AddKnowledgeBase()`: lo compartido (clientes Aspire, repositorio, `KnowledgeStore`, `KbOptions`).
  - `AddKnowledgeIngest()`: lo específico de la ingesta (chunking, `DocumentIngest`, `MarkdownSourceFile`).
  - `AddKnowledgeMcp()`: lo específico del MCP server (servicio de búsqueda).
  - `KbDbContext` usa `EnsureCreated` (sin migraciones por ahora). Las implementaciones son `internal`; solo se exponen las interfaces y los métodos de registro.
- **Kb.McpServer**: tools, resources y prompts descubiertos por atributos (`WithToolsFromAssembly`, `WithResourcesFromAssembly`, `WithPromptsFromAssembly`).
- **Kb.Ingest**: console app que corre `InicializarBaseAsync()` (EnsureCreated) y luego `ProcessIngestAsync()`.

### Nombres de recursos Aspire = claves de connection string

`"qdrant"`, `"kbdb"` y `"embeddings"` se declaran en `AppHost.cs` y deben coincidir con los `AddQdrantClient("qdrant")`, `AddSqlServerDbContext<KbDbContext>("kbdb")` y `AddOllamaApiClient("embeddings")` de `DependencyInjection.cs`. Cambiar un nombre rompe la inyección.

### Lifetimes de DI

- Singleton: `QdrantClient`, `IEmbeddingGenerator`, `KnowledgeStore`, `ChunkingManager`, `DocumentIngest`, `MarkdownSourceFile`.
- Scoped: `KbDbContext` (pooling de Aspire: su constructor solo recibe `DbContextOptions`), `IDocumentRepository` y el servicio de búsqueda del MCP (usa el repositorio).
- Un singleton nunca recibe un servicio scoped. `DocumentIngest` crea un scope por documento con `IServiceScopeFactory` para obtener el repositorio.

### Pipeline de ingesta (Kb.Core/Ingestion + Chunking, Kb.Infrastructure/VectorStore)

1. `MarkdownSourceFile` recorre `docs/**/*.md`; la primera subcarpeta es la categoría (si no hay, `"general"`).
2. `DocumentIngest` calcula SHA-256 del contenido; si el hash no cambió y el documento está `Complete`, lo saltea (idempotencia). Un documento en `Proccess` o `Error` se reintenta. El estado se persiste en SQL Server antes y después de procesar. Errores de un documento se registran en `LastError` sin frenar al resto; el `catch` filtra con `when (!ct.IsCancellationRequested)` para que un timeout de HTTP (que lanza `TaskCanceledException`) cuente como error y no como cancelación.
3. `ChunkingManager.FragmentMarkDown`: el primer `#` es el título del documento (respaldo: nombre del archivo), cada `##` abre una sección.
4. Embeddings en lotes chicos (el modelo es pesado y en CPU puede superar los timeouts de la política de resiliencia de ServiceDefaults).
5. `KnowledgeStore`: colección Qdrant `kb_chunks` (coseno, dimensión tomada del primer embedding, índices de payload en `documentoId` y `categoria`). El update borra los puntos del documento por `documentoId` y reinserta. Payload: `documentoId`, `titulo`, `seccion`, `categoria`, `indice`, `texto`.

El `DocumentId` de SQL es el `documentoId` del payload en Qdrant: las dos bases tienen que estar siempre sincronizadas.

### Búsqueda

- El servicio de búsqueda genera el embedding de la consulta y consulta Qdrant con `QueryAsync`, filtro opcional por categoría (normalizada a minúsculas) y `scoreThreshold` = `Kb:ScoreMinimo`.
- `top` se limita a 1–20.
- Una consulta fuera de tema devuelve lista vacía; la descripción de la tool le indica a Claude que eso significa que la información no está en la base.

### Primitivas MCP expuestas

- **Tools**: `busqueda-de-conocimiento` (búsqueda semántica), `lista-de-documentos` (id, título y categoría, sin contenido) y `obtener-documento` (contenido completo; si no existe, `McpException` con un mensaje que indica cómo seguir).
- **Resources**: `kb://indice` (índice en Markdown agrupado por categoría, con la URI de cada documento) y la plantilla `kb://documento/{id}` (contenido completo). El contenido sale de SQL Server, no de los archivos.
- **Prompts**: `responder_con_fuentes` (obliga a buscar, seguir referencias entre guías, citar título y sección, y decir cuando algo no está en la base).

### Configuración

`KbOptions` (sección `"Kb"`): colección, tamaño máximo de chunk, prefijos de embedding y `ScoreMinimo`. Tiene defaults en código; se sobrescribe en el `appsettings.json` de cada ejecutable (el de `Kb.Ingest` necesita `CopyToOutputDirectory`).

- Con `bge-m3` los prefijos van vacíos (son propios de `nomic-embed-text`).
- Los scores no son comparables entre modelos: cambiar de modelo requiere recalibrar `ScoreMinimo`. Con `bge-m3`, consultas relevantes dan ~0,5 o más y fuera de tema ~0,3; el umbral actual es 0,40.

### Reingesta

Cambiar el modelo de embeddings o la lógica del chunker requiere borrar la colección `kb_chunks` **y** los documentos de SQL (siempre las dos juntas: si se borra solo SQL, los `DocumentId` nuevos dejan chunks huérfanos duplicados en Qdrant). Cambiar la estructura de la entidad requiere borrar la base completa, porque `EnsureCreated` no actualiza tablas existentes.

### Logging

Mensajes con `[LoggerMessage]` (source generator): `IngestLog` en Core para la ingesta, y una clase propia en `Kb.McpServer` para las tools. No se loguea el texto de las consultas (puede contener datos sensibles); solo categoría, cantidad de resultados y mejor score.

## Estado actual / pendientes conocidos

- `Attempts` no se incrementa al marcar un documento como en proceso.
- `CountChunks` se asigna antes de completar el procesamiento (un documento en `Proccess` puede mostrar chunks).
- El texto de cada chunk incluye el contexto `"Titulo > Seccion"`; debería guardarse limpio y armarse el contexto solo para el embedding.
- `FragmentMarkDown` todavía no parte secciones largas por `MaxCaracteresChunk`.
- Documentos borrados de la carpeta dejan registros y chunks huérfanos.
- Sin tests unitarios (chunker y entidad `Document` son los primeros candidatos).
- Migraciones de EF Core en lugar de `EnsureCreated`.

## Convenciones

- Comentarios y nombres de dominio en español; código en PascalCase/camelCase según convención C#.
- Commits estilo Conventional Commits (`feat|fix|chore|refactor|docs|test`).
- Ninguna clase de Core lleva "Mcp" en el nombre: Core no sabe quién la consume.
