# RunbookForge — PostgreSQL Standards & Migration Policies
**Document ID:** `docs/standards/POSTGRES-SQL-STANDARDS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** PostgreSQL Standards & Indexing Policies (`standards/POSTGRES-SQL-STANDARDS.md`)
> - **Global Invariants:** [../04-CONVENTIONS-AND-GUARDRAILS.md](../04-CONVENTIONS-AND-GUARDRAILS.md)  
> - **Database Master Plan:** [../database/DB-00-MASTER-PLAN.md](../database/DB-00-MASTER-PLAN.md)  
> - **Module Schemas:** [../database/DB-01-SCHEMA-RUNBOOKS.md](../database/DB-01-SCHEMA-RUNBOOKS.md) through DB-05

---

## 1. Schema Isolation & Migration History

PostgreSQL serves as the primary relational persistence layer. To maintain modular independence within the single physical database:

1. **Dedicated Migration Histories:** Each module's `DbContext` isolates its migrations history table within its own schema namespace:
   ```csharp
   options.UseNpgsql(dataSource, npgsqlOptions =>
   {
       npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "runbooks");
       npgsqlOptions.MigrationsAssembly("RunbookForge.Modules.RunbookEngine.Infrastructure");
   });
   ```
2. **Schema Creation Guard:** Every migration begins with an idempotent schema creation command:
   ```sql
   CREATE SCHEMA IF NOT EXISTS runbooks;
   ```
3. **No Cross-Schema Foreign Keys:** Tables in `runbooks` cannot define foreign key constraints to tables in `ingestion` or `analytics`.

---

## 2. Naming Conventions

All database identifiers must follow these strict casing rules:

| Object Type | Naming Standard | Example |
| :--- | :--- | :--- |
| **Schemas** | Singular lowercase noun | `runbooks`, `ingestion`, `analytics`, `gaps`, `training` |
| **Tables** | Plural `snake_case` noun | `runbooks.runbook_documents`, `ingestion.sync_jobs` |
| **Columns** | Singular `snake_case` | `id`, `runbook_id`, `created_at`, `is_active` |
| **Primary Keys** | `pk_{table_name}` | `pk_runbook_documents` |
| **Foreign Keys** | `fk_{table}_{referenced_table}` | `fk_versions_runbook_documents` |
| **Indexes** | `ix_{table}_{column(s)}` | `ix_runbook_documents_slug` |
| **GIN Indexes** | `gin_{table}_{column}` | `gin_versions_parsed_ast` |
| **HNSW Indexes** | `hnsw_{table}_{column}` | `hnsw_ticket_embeddings_vector` |

---

## 3. Indexing Strategies & Performance

### 3.1 Foreign Key Columns
Every foreign key column must have a supporting B-Tree index to eliminate full table scans during joins and cascade lookups:
```sql
CREATE INDEX ix_runbook_versions_document_id 
ON runbooks.runbook_versions (document_id);
```

### 3.2 JSONB Columns (GIN Indexing)
Semi-structured payloads (`parsed_ast`, `workflow_graph`, `diagnostics`) must be indexed using Generalized Inverted Indexes (GIN) for high-speed json containment queries (`@>`):
```sql
CREATE INDEX gin_runbook_versions_parsed_ast 
ON runbooks.runbook_versions USING gin (parsed_ast);
```

### 3.3 Vector & Semantic Embeddings (`pgvector` with Full-Text Fallback)
For semantic clustering and similarity matching:
1. **Primary (`pgvector`):** Uses Hierarchical Navigable Small World (HNSW) index with cosine distance operator (`vector_cosine_ops`) over `vector(1536)` columns.
2. **Fallback (Native Full-Text Search with `tsvector`):** If the PostgreSQL runtime lacks the `vector` extension, full-text token search is activated with GIN indexes on synthesized search vectors (`to_tsvector('english', ...)`).

For complete table definitions, conditional extension scripts, and EF Core degraded fallback logic, see the canonical [analytics Schema Specification](../database/DB-03-SCHEMA-ANALYTICS.md#3-vector-embeddings-pgvector-with-native-full-text-fallback).

---

## 4. MVCC & Optimistic Concurrency

All mutable tables must leverage PostgreSQL's native system column `xmin` for lightweight, zero-overhead optimistic concurrency control:

```csharp
// Entity Framework Core Mapping
builder.Property<uint>("Version")
    .IsRowVersion()
    .HasColumnName("xmin");
```

When an update collision occurs, EF Core throws a `DbUpdateConcurrencyException`, which is caught by the MediatR handler and converted to `Result.Failure(Error.Conflict(...))`.
