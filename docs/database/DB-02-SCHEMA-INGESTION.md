# RunbookForge — Database Schema: Ticket Ingestion (`ingestion`)
**Document ID:** `docs/database/DB-02-SCHEMA-INGESTION.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** "ingestion" PostgreSQL Schema DDL (`database/DB-02-SCHEMA-INGESTION.md`)
> - **Database Master Plan:** [./DB-00-MASTER-PLAN.md](./DB-00-MASTER-PLAN.md)  
> - **F7Hub Security Barrier:** [./DB-06-EXTERNAL-F7HUB-READONLY.md](./DB-06-EXTERNAL-F7HUB-READONLY.md)  
> - **PostgreSQL Standards:** [../standards/POSTGRES-SQL-STANDARDS.md](../standards/POSTGRES-SQL-STANDARDS.md)  
> - **Module Registry & Contracts:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)

---

## 1. Schema Overview

- **Schema Name:** `ingestion`
- **Owning Module:** `RunbookForge.Modules.TicketIngestion`
- **Context:** `IngestionDbContext`

---

## 2. Table Definitions & PostgreSQL DDL

```sql
CREATE SCHEMA IF NOT EXISTS ingestion;

-- 1. Sync Jobs Table
CREATE TABLE ingestion.sync_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_system VARCHAR(100) NOT NULL, -- e.g., 'F7HubSQLite', 'ServiceNow'
    status VARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Running', 'Completed', 'Failed'
    processed_count INT NOT NULL DEFAULT 0,
    started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    error_message TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_sync_jobs_status ON ingestion.sync_jobs (status);
CREATE INDEX ix_sync_jobs_source ON ingestion.sync_jobs (source_system);

-- 2. Staged Raw Tickets Table
CREATE TABLE ingestion.raw_tickets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sync_job_id UUID NOT NULL REFERENCES ingestion.sync_jobs(id) ON DELETE CASCADE,
    external_id VARCHAR(255) NOT NULL,
    title VARCHAR(500) NOT NULL,
    description TEXT NOT NULL,
    resolution_notes TEXT,
    severity VARCHAR(50) NOT NULL,
    content_hash VARCHAR(64) NOT NULL, -- SHA256 of title + description + resolution
    raw_payload JSONB NOT NULL DEFAULT '{}',
    created_at_utc TIMESTAMPTZ NOT NULL,
    resolved_at_utc TIMESTAMPTZ,
    ingested_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_raw_tickets_job_id ON ingestion.raw_tickets (sync_job_id);
CREATE INDEX ix_raw_tickets_external_id ON ingestion.raw_tickets (external_id);
CREATE INDEX ix_raw_tickets_hash ON ingestion.raw_tickets (content_hash);

-- 3. Ingestion Deduplication Hashes Table
CREATE TABLE ingestion.ticket_hashes (
    source_system VARCHAR(100) NOT NULL,
    content_hash VARCHAR(64) NOT NULL,
    ticket_id UUID NOT NULL,
    first_seen_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (source_system, content_hash)
);
```
