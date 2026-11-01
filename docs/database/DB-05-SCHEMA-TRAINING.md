# RunbookForge — Database Schema: Training & Critic (`training`)
**Document ID:** `docs/database/DB-05-SCHEMA-TRAINING.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** "training" PostgreSQL Schema DDL (`database/DB-05-SCHEMA-TRAINING.md`)
> - **Database Master Plan:** [./DB-00-MASTER-PLAN.md](./DB-00-MASTER-PLAN.md)  
> - **PostgreSQL Standards:** [../standards/POSTGRES-SQL-STANDARDS.md](../standards/POSTGRES-SQL-STANDARDS.md)  
> - **Module Registry & Contracts:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)  
> - **AI Integration Standards:** [../standards/AI-INTEGRATION-STANDARDS.md](../standards/AI-INTEGRATION-STANDARDS.md)

---

## 1. Schema Overview

- **Schema Name:** `training`
- **Owning Module:** `RunbookForge.Modules.AICriticAndTraining`
- **Context:** `TrainingDbContext`

---

## 2. Table Definitions & PostgreSQL DDL

```sql
CREATE SCHEMA IF NOT EXISTS training;

-- 1. Synthetic Practice Scenarios Table
CREATE TABLE training.practice_scenarios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    linked_runbook_id UUID NOT NULL, -- Logical reference to runbooks.documents
    difficulty_tier VARCHAR(50) NOT NULL, -- 'Beginner', 'Intermediate', 'Advanced'
    injected_faults JSONB NOT NULL DEFAULT '[]',
    expected_resolution_path JSONB NOT NULL DEFAULT '[]',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_practice_scenarios_runbook ON training.practice_scenarios (linked_runbook_id);
CREATE INDEX ix_practice_scenarios_difficulty ON training.practice_scenarios (difficulty_tier);

-- 2. Operator Drill Sessions Table
CREATE TABLE training.drill_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    scenario_id UUID NOT NULL REFERENCES training.practice_scenarios(id) ON DELETE CASCADE,
    operator_user_id VARCHAR(100) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Active', -- 'Active', 'Completed', 'Abandoned'
    current_step_id VARCHAR(100) NOT NULL,
    step_history JSONB NOT NULL DEFAULT '[]',
    started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ
);

CREATE INDEX ix_drill_sessions_operator ON training.drill_sessions (operator_user_id);
CREATE INDEX ix_drill_sessions_status ON training.drill_sessions (status);

-- 3. Drill Evaluation Logs Table
CREATE TABLE training.evaluation_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES training.drill_sessions(id) ON DELETE CASCADE,
    operator_user_id VARCHAR(100) NOT NULL,
    overall_score DOUBLE PRECISION NOT NULL,
    total_steps_completed INT NOT NULL,
    mistakes_count INT NOT NULL,
    average_step_latency_ms BIGINT NOT NULL,
    rubric_breakdown JSONB NOT NULL DEFAULT '{}',
    evaluated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_evaluation_logs_operator ON training.evaluation_logs (operator_user_id);

-- 4. AI Critic Reviews Table
CREATE TABLE training.critic_reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    runbook_id UUID NOT NULL,
    version_number INT NOT NULL,
    completeness_score DOUBLE PRECISION NOT NULL,
    clarity_score DOUBLE PRECISION NOT NULL,
    safety_score DOUBLE PRECISION NOT NULL,
    review_payload JSONB NOT NULL DEFAULT '{}',
    reviewed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_critic_reviews_runbook ON training.critic_reviews (runbook_id, version_number);
```
