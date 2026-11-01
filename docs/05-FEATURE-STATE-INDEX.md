# RunbookForge — Living Feature State Index & Task Tracker
**Document ID:** `docs/05-FEATURE-STATE-INDEX.md`  
**Version:** 1.1.0  
**Status:** ACTIVE  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** Feature State Tracking & Task Backlog (`05-FEATURE-STATE-INDEX.md`)
> - **Implementation SOP:** [03-AI-FEATURE-SOP.md](./03-AI-FEATURE-SOP.md)  
> - **System Architecture:** [00-ARCHITECTURE.md](./00-ARCHITECTURE.md)  
> - **Conventions & Guardrails:** [04-CONVENTIONS-AND-GUARDRAILS.md](./04-CONVENTIONS-AND-GUARDRAILS.md)

---

## 1. Feature Lifecycle & State Taxonomy

All tasks progress through a deterministic lifecycle:
- **`PLANNED`**: Fully specified in documentation matrix; ready for atomic implementation slice.
- **`IN_PROGRESS`**: Actively being implemented by an agent or developer.
- **`BLOCKED`**: Pre-requisite contract or database migration dependency pending.
- **`COMPLETED`**: Code, migrations, tests, and verification scripts committed and verified.

---

## 2. Master Feature Tracking Matrix

### 2.1 Foundation & Core Infrastructure (`Core`)

| Task ID | Task Title & Scope | Target Layer | Context Files | Status | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `RF-CORE-001` | Scaffold Solution Directory Layout & `.csproj` References | `BuildingBlocks` | `00-ARCH`, `04-GUARD` | `COMPLETED` | Verified: 19 .NET projects build with 0 warnings/errors, Vite frontend builds with 0 type errors. |
| `RF-CORE-002` | Implement Result Pattern & Error Handling Primitives | `BuildingBlocks.Common` | `04-GUARD`, `CS-STD` | `COMPLETED` | Verified: 30 xUnit tests passed (Result, Behaviors, RFC 7807 mappings). |
| `RF-CORE-003` | MediatR Pipeline Behaviors (Logging, Validation, Transaction) | `BuildingBlocks.Common` | `02-EVENTS`, `CS-STD` | `COMPLETED` | Embedded with FluentValidation & LoggingBehavior unit test coverage. |
| `RF-CORE-004` | PostgreSQL Connection Pooling & NpgsqlDataSource Setup | `BuildingBlocks.Persistence` | `DB-00`, `PG-STD` | `PLANNED` | Schema search path configuration. |
| `RF-CORE-005` | WebApi Composition Root & Minimal API Dynamic Discovery | `WebApi` | `00-ARCH`, `01-REG` | `COMPLETED` | Dynamic IEndpointModule discovery, Swagger, and health checks initialized. |

---

### 2.2 Module: RunbookEngine (`runbooks`)

| Task ID | Task Title & Scope | Target Schema | Context Files | Status | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `RF-RUNBOOK-001` | `runbooks` PostgreSQL Schema Migrations & EF Core DbContext | `runbooks` | `DB-01`, `PG-STD` | `COMPLETED` | Verified: RunbookDbContext, 4 entities, EF Core migration Initial_Runbooks_Schema with xmin concurrency & GIN indexes. |
| `RF-RUNBOOK-002` | Markdig AST Pipeline & Custom Container Block Parsers | `RunbookEngine.Infra` | `MD-STD`, `CS-STD` | `COMPLETED` | Verified: Custom Markdig AST container parsers, nested block handling, code fence attributes, 7/7 xUnit tests passed. |
| `RF-RUNBOOK-003` | Diagnostic Linter Engine (`RF001`–`RF099` Rules) | `RunbookEngine.Domain` / `RunbookEngine.App` | `MD-STD`, `01-REG` | `IN_PROGRESS` | Verified: Batch A rules RF001–RF015 implemented & tested (RunbookLinterEngine, LintRunbookQuery/Handler, score arithmetic, 82 xUnit tests passed). |
| `RF-RUNBOOK-004` | AST to Workflow DAG Compiler & React Flow Schema Serialization | `RunbookEngine.Domain` / `RunbookEngine.App` | `MD-STD`, `01-REG` | `COMPLETED` | Verified: `WorkflowDagCompiler` (3-colour DFS cycle detection, Kahn ranking, lane layout), `ReactFlowSerializer`, `CompileRunbookWorkflowHandler` persisting `runbooks.workflows`; 36 workflow tests passed. |
| `RF-RUNBOOK-005` | React Flow Step Graph Visualizer & Markdown Editor UI | `Frontend` | `TS-STD`, `01-REG` | `PLANNED` | Interactive step graph canvas with inline lint squiggles. |
| `RF-RUNBOOK-006` | Runbook Draft & Version Publication MediatR Handlers | `RunbookEngine.App` | `01-REG`, `03-SOP` | `PLANNED` | Command handlers with optimistic concurrency (`xmin`). Renumbered from the former `RF-RUNBOOK-004`. |

---

### 2.3 Module: TicketIngestion (`ingestion`)

| Task ID | Task Title & Scope | Target Schema | Context Files | Status | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `RF-INGEST-001` | `ingestion` PostgreSQL Schema Migrations & DbContext | `ingestion` | `DB-02`, `PG-STD` | `PLANNED` | `sync_jobs`, `raw_tickets`, `ticket_hashes`. |
| `RF-INGEST-002` | Read-Only F7Hub SQLite Adapter (`F7HubReadOnlyDbContext`) | `TicketIngestion.Infra` | `DB-06`, `04-GUARD` | `PLANNED` | Enforce `Data Source=f7hub.db;Mode=ReadOnly;Cache=Shared;` and exception on `SaveChanges`. |
| `RF-INGEST-003` | Incident Sync Worker & Content Deduplication Pipeline | `TicketIngestion.App` | `01-REG`, `02-EVENTS` | `PLANNED` | SHA256 content hashing & `TicketBatchIngestedEvent`. |

---

### 2.4 Module: PatternAnalytics (`analytics`)

| Task ID | Task Title & Scope | Target Schema | Context Files | Status | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `RF-ANALYTICS-001` | `analytics` PostgreSQL Schema Migrations & Vector Setup | `analytics` | `DB-03`, `PG-STD` | `PLANNED` | Tables + `pgvector` HNSW index with `tsvector` fallback. |
| `RF-ANALYTICS-002` | Ticket Symptom Clustering & Embedding Generation Engine | `PatternAnalytics.App` | `01-REG`, `AI-STD` | `PLANNED` | Cluster incident transcripts and emit discovery events. |
| `RF-ANALYTICS-003` | Runbook Execution Telemetry & MTTR Calculation Handlers | `PatternAnalytics.App` | `01-REG`, `02-EVENTS` | `PLANNED` | Aggregate step durations and success ratios. |

---

### 2.5 Module: GapAnalyzer (`gaps`)

| Task ID | Task Title & Scope | Target Schema | Context Files | Status | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `RF-GAPS-001` | `gaps` PostgreSQL Schema Migrations & DbContext | `gaps` | `DB-04`, `PG-STD` | `PLANNED` | `coverage_reports`, `missing_patterns`. |
| `RF-GAPS-002` | Coverage Evaluation Engine (Cluster vs Runbook Mapping) | `GapAnalyzer.App` | `01-REG`, `02-EVENTS` | `PLANNED` | Compare unmapped clusters to published runbooks. |
| `RF-GAPS-003` | Operational Gap Dashboard & Prioritization View | `Frontend` | `TS-STD`, `01-REG` | `PLANNED` | Subsystem coverage matrix and gap closure metrics. |

---

### 2.6 Module: AICriticAndTraining (`training`)

| Task ID | Task Title & Scope | Target Schema | Context Files | Status | Notes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `RF-TRAINING-001` | `training` PostgreSQL Schema Migrations & DbContext | `training` | `DB-05`, `PG-STD` | `PLANNED` | `practice_scenarios`, `evaluation_logs`. |
| `RF-TRAINING-002` | AI Critic Review Engine (LLM Rubric Evaluation) | `AICriticAndTraining.App` | `AI-STD`, `01-REG` | `PLANNED` | Schema-enforced completeness, clarity, and safety scoring. |
| `RF-TRAINING-003` | Operator Drill Simulation Engine & Step Scorer | `AICriticAndTraining.App` | `01-REG`, `02-EVENTS` | `PLANNED` | Interactive incident response drill runner. |

---

## 3. Milestones & Delivery Roadmap

```mermaid
gantt
    title RunbookForge Milestone Roadmap
    dateFormat YYYY-MM-DD
    section Foundation
    M1: Core Architecture & BuildingBlocks :milestone, m1, 2026-11-01, 1d
    section Ingestion & Parser
    M2: F7Hub Read-Only Sync & Ticket Ingestion :m2, 2026-11-02, 3d
    M3: Markdig AST & RF001-RF099 Linting Engine :m3, 2026-11-05, 4d
    section Analytics & Training
    M4: Telemetry, pgvector Clustering & Gap Engine :m4, 2026-11-09, 5d
    M5: Interactive Drills & AI Critic Engine :m5, 2026-11-14, 5d
```
