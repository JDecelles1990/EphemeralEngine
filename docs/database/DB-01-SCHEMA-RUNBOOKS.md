# RunbookForge — Database Schema: Runbooks (`runbooks`)
**Document ID:** `docs/database/DB-01-SCHEMA-RUNBOOKS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** "runbooks" PostgreSQL Schema DDL (`database/DB-01-SCHEMA-RUNBOOKS.md`)
> - **Database Master Plan:** [./DB-00-MASTER-PLAN.md](./DB-00-MASTER-PLAN.md)  
> - **PostgreSQL Standards:** [../standards/POSTGRES-SQL-STANDARDS.md](../standards/POSTGRES-SQL-STANDARDS.md)  
> - **Module Registry & Contracts:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)  
> - **Markdown Parser Spec:** [../standards/MARKDOWN-PARSER-STANDARDS.md](../standards/MARKDOWN-PARSER-STANDARDS.md)

---

## 1. Schema Overview

- **Schema Name:** `runbooks`
- **Owning Module:** `RunbookForge.Modules.RunbookEngine`
- **Context:** `RunbooksDbContext`

---

## 2. Table Definitions & PostgreSQL DDL

### Documents Table
<a id="documents-table"></a>

```sql
CREATE SCHEMA IF NOT EXISTS runbooks;

-- 1. Master Documents Table
CREATE TABLE runbooks.documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    description TEXT NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Draft', -- 'Draft', 'UnderReview', 'Published', 'Archived'
    tags TEXT[] NOT NULL DEFAULT '{}',
    current_version_number INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    xmin XID
);

CREATE INDEX ix_documents_slug ON runbooks.documents (slug);
CREATE INDEX ix_documents_status ON runbooks.documents (status);
CREATE INDEX gin_documents_tags ON runbooks.documents USING gin (tags);

-- 2. Immutable Runbook Versions Snapshot Table
CREATE TABLE runbooks.versions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL REFERENCES runbooks.documents(id) ON DELETE CASCADE,
    version_number INT NOT NULL,
    raw_markdown TEXT NOT NULL,
    parsed_ast JSONB NOT NULL,
    change_summary TEXT NOT NULL,
    published_by_user_id VARCHAR(100) NOT NULL,
    published_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_versions_document_version UNIQUE (document_id, version_number)
);

CREATE INDEX ix_versions_document_id ON runbooks.versions (document_id);
CREATE INDEX gin_versions_parsed_ast ON runbooks.versions USING gin (parsed_ast);

-- 3. Lint Diagnostic Execution Results Table
CREATE TABLE runbooks.lint_results (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL REFERENCES runbooks.documents(id) ON DELETE CASCADE,
    version_number INT NOT NULL,
    is_valid BOOLEAN NOT NULL,
    error_count INT NOT NULL DEFAULT 0,
    warning_count INT NOT NULL DEFAULT 0,
    diagnostics JSONB NOT NULL DEFAULT '[]',
    evaluated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_lint_results_document_id ON runbooks.lint_results (document_id);

-- 4. React Flow Workflow Topology Table
CREATE TABLE runbooks.workflows (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL REFERENCES runbooks.documents(id) ON DELETE CASCADE,
    version_number INT NOT NULL,
    nodes JSONB NOT NULL DEFAULT '[]',
    edges JSONB NOT NULL DEFAULT '[]',
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_workflows_document_version UNIQUE (document_id, version_number)
);

CREATE INDEX gin_workflows_nodes ON runbooks.workflows USING gin (nodes);
CREATE INDEX gin_workflows_edges ON runbooks.workflows USING gin (edges);
```

---

## 3. JSONB Structures

### `runbooks.versions.parsed_ast`
```json
{
  "title": "Database Connection Pool Recovery",
  "slug": "db-connection-pool-recovery",
  "version": 1,
  "steps": [
    {
      "stepId": "step-1",
      "stepOrder": 1,
      "stepType": "Interactive",
      "title": "Inspect Active Connections",
      "commandScript": "psql -c 'SELECT count(*) FROM pg_stat_activity;'",
      "expectedOutcome": "Integer connection count",
      "nextStepIds": ["step-2"]
    }
  ]
}
```
