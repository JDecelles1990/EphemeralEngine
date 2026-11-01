# RunbookForge — Data Flow, Sequences & Event Specifications
**Document ID:** `docs/02-DATA-FLOW-AND-EVENTS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** Sequence Flows & Event Payloads (`02-DATA-FLOW-AND-EVENTS.md`)
> - **Public Contracts:** [01-MODULE-REGISTRY.md](./01-MODULE-REGISTRY.md)  
> - **Implementation SOP:** [03-AI-FEATURE-SOP.md](./03-AI-FEATURE-SOP.md)  
> - **Database Master Plan:** [database/DB-00-MASTER-PLAN.md](./database/DB-00-MASTER-PLAN.md)

---

## 1. Event Dispatching Architecture

RunbookForge uses **MediatR In-Process Notifications** (`INotification` / `INotificationHandler<T>`) for cross-module side effects. This ensures:
1. **Decoupled Execution:** Modules do not know about other modules' handlers or database models.
2. **Transactional Boundary Safety:** A command handler commits its own module's `DbContext` transaction *before* publishing domain events. Downstream event handlers execute in independent transaction scopes.
3. **Resilience:** Failure in an asynchronous downstream notification handler does not roll back the upstream primary mutation.

```mermaid
graph LR
    subgraph ModuleA ["Originating Module (e.g., TicketIngestion)"]
        Cmd["Handle(Command)"] --> Save["DbContext.SaveChangesAsync()"]
        Save --> Publish["Publisher.Publish(Event)"]
    end

    subgraph MediatRPipeline ["MediatR In-Process Bus"]
        Publish --> Dispatch{"Dispatch to Handlers"}
    end

    subgraph ModuleB ["Subscribing Module (e.g., PatternAnalytics)"]
        Dispatch --> HandlerB["Handle(Event, ct)"]
        HandlerB --> SaveB["AnalyticsDbContext.SaveChangesAsync()"]
    end

    subgraph ModuleC ["Subscribing Module (e.g., GapAnalyzer)"]
        Dispatch --> HandlerC["Handle(Event, ct)"]
        HandlerC --> SaveC["GapsDbContext.SaveChangesAsync()"]
    end
```

---

## 2. Core Cross-Module Sequence Diagrams

### Flow 1: F7Hub Ingestion -> Pattern Mining -> Operational Gap Detection

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Operations Engineer
    participant API as WebApi Gateway
    participant Ingestion as TicketIngestion Module
    participant F7Hub as F7Hub SQLite (ReadOnly)
    participant Bus as MediatR In-Process Bus
    participant Analytics as PatternAnalytics Module
    participant Gaps as GapAnalyzer Module

    Admin->>API: POST /api/ingestion/sync/f7hub
    API->>Ingestion: Send(TriggerF7HubSyncCommand)
    Ingestion->>F7Hub: Stream new incidents (Mode=ReadOnly)
    F7Hub-->>Ingestion: Return records
    Ingestion->>Ingestion: Deduplicate via SHA256 hashes
    Ingestion->>Ingestion: IngestionDbContext.SaveChangesAsync()
    Ingestion->>Bus: Publish(TicketBatchIngestedEvent)
    Ingestion-->>API: Result.Success(SyncJobSummaryDto)
    API-->>Admin: 202 Accepted (SyncJobSummaryDto)

    par Asynchronous Event Processing
        Bus->>Analytics: Handle(TicketBatchIngestedEvent)
        Analytics->>Analytics: Extract symptom tokens & compute embeddings
        Analytics->>Analytics: Cluster tickets & persist to analytics.symptom_clusters
        Analytics->>Bus: Publish(SymptomClusterDiscoveredEvent)
    and
        Bus->>Gaps: Handle(SymptomClusterDiscoveredEvent)
        Gaps->>Gaps: Check runbook coverage against cluster signature
        opt No Runbook Covers Cluster
            Gaps->>Gaps: Persist gap to gaps.detected_gaps
            Gaps->>Bus: Publish(OperationalGapIdentifiedEvent)
        end
    end
```

---

### Flow 2: Runbook Authoring -> Markdig Linting -> AI Critic Review -> Version Publication

```mermaid
sequenceDiagram
    autonumber
    actor Author as Runbook Author
    participant WebApp as React 19 UI
    participant API as WebApi Gateway
    participant Runbooks as RunbookEngine Module
    participant Critic as AICriticAndTraining Module
    participant Bus as MediatR In-Process Bus
    participant Gaps as GapAnalyzer Module

    Author->>WebApp: Edit Markdown content
    WebApp->>API: POST /api/runbooks/lint
    API->>Runbooks: Send(LintRunbookContentCommand)
    Runbooks->>Runbooks: Parse Markdig AST & Run RF001-RF099 Rules
    Runbooks-->>WebApp: Result.Success(RunbookLintReportDto)
    WebApp-->>Author: Display inline diagnostic squiggles

    Author->>WebApp: Request AI Critic Review
    WebApp->>API: POST /api/runbooks/{id}/critic-review
    API->>Critic: Send(RequestAICriticReviewCommand)
    Critic->>Critic: Analyze AST & prompt LLM with Rubric Schema
    Critic-->>WebApp: Result.Success(AICriticReviewResultDto)

    Author->>WebApp: Click "Publish Version"
    WebApp->>API: POST /api/runbooks/{id}/publish
    API->>Runbooks: Send(PublishRunbookVersionCommand)
    Runbooks->>Runbooks: Freeze immutable version in runbooks.runbook_versions
    Runbooks->>Runbooks: RunbooksDbContext.SaveChangesAsync()
    Runbooks->>Bus: Publish(RunbookPublishedEvent)
    Runbooks-->>WebApp: Result.Success(PublishedRunbookVersionDto)

    Bus->>Gaps: Handle(RunbookPublishedEvent)
    Gaps->>Gaps: Re-evaluate open operational gaps
    Gaps->>Gaps: Close resolved gaps in gaps.detected_gaps
```

---

### Flow 3: Interactive Operator Drill -> Step Telemetry -> Efficiency Metrics

```mermaid
sequenceDiagram
    autonumber
    actor Operator as Operator Trainee
    participant WebApp as React Flow Drill View
    participant API as WebApi Gateway
    participant Training as AICriticAndTraining Module
    participant Bus as MediatR In-Process Bus
    participant Analytics as PatternAnalytics Module

    Operator->>WebApp: Start Simulation Drill
    WebApp->>API: POST /api/training/drills/start
    API->>Training: Send(StartOperatorDrillCommand)
    Training-->>WebApp: Result.Success(ActiveDrillSessionDto)

    loop Each Incident Step
        Operator->>WebApp: Execute diagnosis or recovery action
        WebApp->>API: POST /api/training/drills/{id}/submit-step
        API->>Training: Send(SubmitDrillStepExecutionCommand)
        Training-->>WebApp: Result.Success(DrillStepFeedbackDto)
    end

    Operator->>WebApp: Complete Final Step
    WebApp->>API: POST /api/training/drills/{id}/finalize
    API->>Training: Send(FinalizeDrillSessionCommand)
    Training->>Training: Compute readiness score & persist evaluation log
    Training->>Bus: Publish(OperatorDrillCompletedEvent)
    Training-->>WebApp: Result.Success(DrillSessionSummaryDto)

    Bus->>Analytics: Handle(OperatorDrillCompletedEvent)
    Analytics->>Analytics: Aggregate operator reaction latencies into step telemetry
```

---

## 3. Concrete Domain Event JSON Schemas

### 3.1 `TicketBatchIngestedEvent`
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "TicketBatchIngestedEvent",
  "type": "object",
  "properties": {
    "eventId": { "type": "string", "format": "uuid" },
    "syncJobId": { "type": "string", "format": "uuid" },
    "sourceCatalog": { "type": "string", "enum": ["F7HubSQLite", "ServiceNowApi", "JiraServiceDesk"] },
    "ingestedTicketCount": { "type": "integer", "minimum": 1 },
    "ingestedTicketIds": {
      "type": "array",
      "items": { "type": "string", "format": "uuid" }
    },
    "completedAtUtc": { "type": "string", "format": "date-time" }
  },
  "required": ["eventId", "syncJobId", "sourceCatalog", "ingestedTicketCount", "ingestedTicketIds", "completedAtUtc"],
  "additionalProperties": false
}
```

### 3.2 `SymptomClusterDiscoveredEvent`
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "SymptomClusterDiscoveredEvent",
  "type": "object",
  "properties": {
    "eventId": { "type": "string", "format": "uuid" },
    "clusterId": { "type": "string", "format": "uuid" },
    "clusterSignature": { "type": "string", "example": "SIG_POSTGRES_CONNECTION_POOL_EXHAUSTION" },
    "ticketFrequency": { "type": "integer", "minimum": 1 },
    "affectedSubsystem": { "type": "string", "example": "CoreDatabasePersistence" },
    "discoveredAtUtc": { "type": "string", "format": "date-time" }
  },
  "required": ["eventId", "clusterId", "clusterSignature", "ticketFrequency", "affectedSubsystem", "discoveredAtUtc"],
  "additionalProperties": false
}
```

### 3.3 `RunbookPublishedEvent`
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "RunbookPublishedEvent",
  "type": "object",
  "properties": {
    "eventId": { "type": "string", "format": "uuid" },
    "runbookId": { "type": "string", "format": "uuid" },
    "versionNumber": { "type": "integer", "minimum": 1 },
    "title": { "type": "string" },
    "slug": { "type": "string" },
    "tags": {
      "type": "array",
      "items": { "type": "string" }
    },
    "publishedAt": { "type": "string", "format": "date-time" }
  },
  "required": ["eventId", "runbookId", "versionNumber", "title", "slug", "tags", "publishedAt"],
  "additionalProperties": false
}
```

### 3.4 `OperationalGapIdentifiedEvent`
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "OperationalGapIdentifiedEvent",
  "type": "object",
  "properties": {
    "eventId": { "type": "string", "format": "uuid" },
    "gapId": { "type": "string", "format": "uuid" },
    "symptomClusterId": { "type": "string", "format": "uuid" },
    "subsystem": { "type": "string" },
    "urgencyTier": { "type": "string", "enum": ["Critical", "High", "Medium", "Low"] },
    "affectedIncidentCount": { "type": "integer", "minimum": 1 },
    "detectedAtUtc": { "type": "string", "format": "date-time" }
  },
  "required": ["eventId", "gapId", "symptomClusterId", "subsystem", "urgencyTier", "affectedIncidentCount", "detectedAtUtc"],
  "additionalProperties": false
}
```
