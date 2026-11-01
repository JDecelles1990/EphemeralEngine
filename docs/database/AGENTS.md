# Agent Instructions: Database & Persistence Architecture

> **Context Dependencies:**  
> - **Source of Truth:** Database Specifications Routing (`database/AGENTS.md`)
> - **Global Agent Instructions:** [../AGENTS.md](../AGENTS.md)  
> - **PostgreSQL Standards:** [../standards/POSTGRES-SQL-STANDARDS.md](../standards/POSTGRES-SQL-STANDARDS.md)  
> - **Scope:** PostgreSQL modular schemas, migrations, indexes, and external read-only SQLite access.

## 1. Schema & Adapter Routing Matrix

| Database Domain / Module | Target Specification File |
| :--- | :--- |
| Overall DB strategy, connection pooling, schema isolation | [`DB-00-MASTER-PLAN.md`](./DB-00-MASTER-PLAN.md) |
| Module 1: `runbooks` schema (documents, AST JSON, lint results) | [`DB-01-SCHEMA-RUNBOOKS.md`](./DB-01-SCHEMA-RUNBOOKS.md) |
| Module 2: `ingestion` schema (raw tickets, sync batches) | [`DB-02-SCHEMA-INGESTION.md`](./DB-02-SCHEMA-INGESTION.md) |
| Module 3: `analytics` schema (clusters, timelines, taxonomies) | [`DB-03-SCHEMA-ANALYTICS.md`](./DB-03-SCHEMA-ANALYTICS.md) |
| Module 4: `gaps` schema (coverage reports, unmapped patterns) | [`DB-04-SCHEMA-GAPS.md`](./DB-04-SCHEMA-GAPS.md) |
| Module 5: `training` schema (scenarios, evaluation logs) | [`DB-05-SCHEMA-TRAINING.md`](./DB-05-SCHEMA-TRAINING.md) |
| External: F7Hub SQLite Read-Only Adapter & Guardrails | [`DB-06-EXTERNAL-F7HUB-READONLY.md`](./DB-06-EXTERNAL-F7HUB-READONLY.md) |

---

## 2. Invariants & Safety Guardrails
- **Read-Only SQLite Rule:** F7Hub data must be accessed exclusively through `F7HubReadOnlyDbContext` with `Mode=ReadOnly;` (see [F7Hub Read-Only Adapter Specification](./DB-06-EXTERNAL-F7HUB-READONLY.md#2-hardened-f7hubreadonlydbcontext-implementation)). Any attempt to execute writes or run migrations against F7Hub must fail at build or runtime.
- **Schema Boundary Isolation:** Never define foreign key constraints linking tables in different PostgreSQL schemas (see [DB-00-MASTER-PLAN.md](./DB-00-MASTER-PLAN.md#2-multi-schema-isolation-policy)). Inter-module associations are maintained purely via application-level UUIDs.

---

## 3. Post-Migration Documentation Protocol
Whenever a database migration or entity configuration is added or modified:
1. **Schema File Update:** Update the corresponding `DB-0X-SCHEMA-*.md` document with the new column definitions, constraints, and index declarations.
2. **JSONB Structure Spec:** If a `jsonb` column is introduced or modified (e.g., in `runbooks.documents` or `analytics.symptom_clusters`), update the corresponding Markdown file with the exact TypeScript/C# JSON schema definition.
