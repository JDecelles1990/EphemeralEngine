# RunbookForge — Database Schema: Gap Analyzer (`gaps`)
**Document ID:** `docs/database/DB-04-SCHEMA-GAPS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** "gaps" PostgreSQL Schema DDL (`database/DB-04-SCHEMA-GAPS.md`)
> - **Database Master Plan:** [./DB-00-MASTER-PLAN.md](./DB-00-MASTER-PLAN.md)  
> - **PostgreSQL Standards:** [../standards/POSTGRES-SQL-STANDARDS.md](../standards/POSTGRES-SQL-STANDARDS.md)  
> - **Module Registry & Contracts:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)  
> - **Analytics Schema:** [./DB-03-SCHEMA-ANALYTICS.md](./DB-03-SCHEMA-ANALYTICS.md)

---

## 1. Schema Overview

- **Schema Name:** `gaps`
- **Owning Module:** `RunbookForge.Modules.GapAnalyzer`
- **Context:** `GapsDbContext`

---

## 2. Table Definitions & PostgreSQL DDL

```sql
CREATE SCHEMA IF NOT EXISTS gaps;

-- 1. System Coverage Assessment Reports Table
CREATE TABLE gaps.coverage_reports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    global_coverage_percentage DOUBLE PRECISION NOT NULL,
    total_known_clusters INT NOT NULL,
    covered_clusters INT NOT NULL,
    uncovered_gaps_count INT NOT NULL,
    subsystem_breakdown JSONB NOT NULL DEFAULT '{}',
    assessed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_coverage_reports_assessed_at ON gaps.coverage_reports (assessed_at DESC);

-- 2. Detected Operational Gaps Table
CREATE TABLE gaps.detected_gaps (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    symptom_cluster_id UUID NOT NULL, -- Logical reference to analytics.symptom_clusters
    subsystem VARCHAR(100) NOT NULL,
    urgency_tier VARCHAR(50) NOT NULL, -- 'Critical', 'High', 'Medium', 'Low'
    affected_incident_count INT NOT NULL,
    description TEXT NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Open', -- 'Open', 'DraftingInProgress', 'Dismissed', 'Resolved'
    resolved_by_runbook_id UUID, -- Logical reference to runbooks.documents
    detected_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    xmin XID
);

CREATE INDEX ix_detected_gaps_subsystem ON gaps.detected_gaps (subsystem);
CREATE INDEX ix_detected_gaps_urgency ON gaps.detected_gaps (urgency_tier);
CREATE INDEX ix_detected_gaps_status ON gaps.detected_gaps (status);

-- 3. Gap Dismissal Audit Logs Table
CREATE TABLE gaps.dismissal_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    gap_id UUID NOT NULL REFERENCES gaps.detected_gaps(id) ON DELETE CASCADE,
    dismissal_reason TEXT NOT NULL,
    dismissed_by_user_id VARCHAR(100) NOT NULL,
    dismissed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_dismissal_logs_gap_id ON gaps.dismissal_logs (gap_id);
```
