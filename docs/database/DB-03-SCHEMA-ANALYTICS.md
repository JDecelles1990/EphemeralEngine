# RunbookForge — Database Schema: Pattern Analytics (`analytics`)
**Document ID:** `docs/database/DB-03-SCHEMA-ANALYTICS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** "analytics" PostgreSQL Schema DDL (`database/DB-03-SCHEMA-ANALYTICS.md`)
> - **Database Master Plan:** [./DB-00-MASTER-PLAN.md](./DB-00-MASTER-PLAN.md)  
> - **PostgreSQL Standards:** [../standards/POSTGRES-SQL-STANDARDS.md](../standards/POSTGRES-SQL-STANDARDS.md)  
> - **Module Registry & Contracts:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)  
> - **AI Integration Standards:** [../standards/AI-INTEGRATION-STANDARDS.md](../standards/AI-INTEGRATION-STANDARDS.md)

---

## 1. Schema Overview

- **Schema Name:** `analytics`
- **Owning Module:** `RunbookForge.Modules.PatternAnalytics`
- **Context:** `AnalyticsDbContext`

---

## 2. Table Definitions & PostgreSQL DDL

```sql
CREATE SCHEMA IF NOT EXISTS analytics;

-- 1. Symptom Clusters Table
CREATE TABLE analytics.symptom_clusters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    cluster_signature VARCHAR(255) NOT NULL UNIQUE,
    top_symptoms TEXT NOT NULL,
    incident_count INT NOT NULL DEFAULT 1,
    assigned_urgency VARCHAR(50) NOT NULL, -- 'Critical', 'High', 'Medium', 'Low'
    affected_subsystem VARCHAR(100) NOT NULL,
    linked_runbook_id UUID, -- Foreign ID without SQL FK constraint
    last_incident_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    search_vector tsvector GENERATED ALWAYS AS (
        to_tsvector('english', title || ' ' || coalesce(top_symptoms, '') || ' ' || affected_subsystem)
    ) STORED
);

CREATE INDEX ix_symptom_clusters_subsystem ON analytics.symptom_clusters (affected_subsystem);
CREATE INDEX ix_symptom_clusters_urgency ON analytics.symptom_clusters (assigned_urgency);
CREATE INDEX gin_symptom_clusters_search ON analytics.symptom_clusters USING gin (search_vector);

-- 2. Step Execution Telemetry Traces Table
CREATE TABLE analytics.timelines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    runbook_id UUID NOT NULL,
    version_number INT NOT NULL,
    execution_session_id UUID NOT NULL,
    step_id VARCHAR(100) NOT NULL,
    step_order INT NOT NULL,
    duration_ms BIGINT NOT NULL,
    exit_code VARCHAR(50) NOT NULL,
    was_successful BOOLEAN NOT NULL,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_timelines_runbook_id ON analytics.timelines (runbook_id);
CREATE INDEX ix_timelines_session_id ON analytics.timelines (execution_session_id);

-- 3. Operational Taxonomy Table
CREATE TABLE analytics.taxonomies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category VARCHAR(100) NOT NULL,
    subsystem VARCHAR(100) NOT NULL,
    common_failure_modes TEXT[] NOT NULL DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX ix_taxonomies_category_subsystem ON analytics.taxonomies (category, subsystem);
```

---

## 3. Vector Embeddings (`pgvector` with Native Full-Text Fallback)
<a id="vector-embeddings"></a>

### 3.1 Primary `pgvector` Strategy
When the PostgreSQL environment has the `vector` extension available, embeddings are stored and searched via HNSW cosine distance:

```sql
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_available_extensions WHERE name = 'vector') THEN
        CREATE EXTENSION IF NOT EXISTS vector;

        CREATE TABLE IF NOT EXISTS analytics.ticket_embeddings (
            ticket_id UUID PRIMARY KEY,
            embedding vector(1536) NOT NULL,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );

        CREATE INDEX IF NOT EXISTS hnsw_ticket_embeddings_vector 
        ON analytics.ticket_embeddings USING hnsw (embedding vector_cosine_ops);
    END IF;
END $$;
```

### 3.2 EF Core Degraded Fallback Logic
Using `Pgvector.EntityFrameworkCore`, the repository checks for vector support at runtime. When absent, it automatically degrades queries to `tsquery` and `ts_rank` against `analytics.symptom_clusters.search_vector`.
