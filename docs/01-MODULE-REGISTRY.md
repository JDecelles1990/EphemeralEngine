# RunbookForge — Public Module Registry & API Contracts
**Document ID:** `docs/01-MODULE-REGISTRY.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** Public API Contracts & DTOs (`01-MODULE-REGISTRY.md`)
> - **System Architecture:** [00-ARCHITECTURE.md](./00-ARCHITECTURE.md)  
> - **Event Flow & Sequences:** [02-DATA-FLOW-AND-EVENTS.md](./02-DATA-FLOW-AND-EVENTS.md)  
> - **Implementation SOP:** [03-AI-FEATURE-SOP.md](./03-AI-FEATURE-SOP.md)  
> - **Persistence Schemas:** [database/DB-00-MASTER-PLAN.md](./database/DB-00-MASTER-PLAN.md)

---

## 1. Overview & Public API Rules

This registry defines the authoritative public contracts for all five RunbookForge bounded contexts. 

### Mandatory Contract Guardrails:
1. **Public Contracts Location:** All cross-module contracts, command records, query records, domain events, and public DTOs reside in `RunbookForge.BuildingBlocks.Common.Contracts`.
2. **Deterministic Return Types:** Every command and query handler returns `Task<Result<T>>` or `Task<Result>`.
3. **Immutability:** All command, query, event, and DTO types are declared as C# `public readonly record struct` or `public sealed record`.
4. **Cancellation Tokens:** All contracts require and propagate `CancellationToken cancellationToken`.

---

## 2. Module 1: RunbookEngine (Schema: `runbooks`)

Responsible for Markdown AST parsing, step execution trees, semantic linting (`RF001`–`RF099`), and immutable version publishing.

### 2.1 Commands & Queries

```csharp
namespace RunbookForge.BuildingBlocks.Common.Contracts.Runbooks;

// --- Commands ---

public sealed record CreateRunbookDraftCommand(
    string Title,
    string Slug,
    string Description,
    string InitialMarkdownContent,
    IReadOnlyList<string> Tags
) : ICommand<Result<RunbookDraftDto>>;

public sealed record UpdateRunbookDraftContentCommand(
    Guid RunbookId,
    int ExpectedVersionNumber,
    string UpdatedMarkdownContent
) : ICommand<Result<RunbookDraftDto>>;

public sealed record LintRunbookContentCommand(
    string MarkdownContent
) : ICommand<Result<RunbookLintReportDto>>;

public sealed record PublishRunbookVersionCommand(
    Guid RunbookId,
    string ChangeSummary,
    string PublishedByUserId
) : ICommand<Result<PublishedRunbookVersionDto>>;

public sealed record CompileRunbookWorkflowCommand(
    Guid DocumentId,
    RunbookAstDocument Ast
) : ICommand<Result<RunbookWorkflowDto>>;

// --- Queries ---

public sealed record GetRunbookByIdQuery(
    Guid RunbookId
) : IQuery<Result<RunbookDetailDto>>;

public sealed record GetRunbookAstQuery(
    Guid RunbookId,
    int? VersionNumber = null
) : IQuery<Result<RunbookAstDto>>;

public sealed record ListRunbooksQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? Tag = null
) : IQuery<Result<PagedList<RunbookSummaryDto>>>;
```

### 2.2 Domain Notifications & DTOs
<a id="runbook-dtos"></a>

```csharp
// --- Domain Events ---

public sealed record RunbookPublishedEvent(
    Guid EventId,
    Guid RunbookId,
    int VersionNumber,
    string Title,
    string Slug,
    IReadOnlyList<string> Tags,
    DateTimeOffset PublishedAt
) : IDomainEvent;

// --- DTOs ---

public sealed record RunbookDraftDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    string RawMarkdown,
    int VersionNumber,
    IReadOnlyList<string> Tags,
    DateTimeOffset UpdatedAt
);

public sealed record RunbookLintReportDto(
    bool IsValid,
    int ErrorCount,
    int WarningCount,
    IReadOnlyList<LintDiagnosticDto> Diagnostics
);

public sealed record LintDiagnosticDto(
    string RuleId, // e.g., "RF021"
    string Severity, // "Error", "Warning", "Info"
    string Message,
    int LineNumber,
    int ColumnNumber,
    string? RemediationSuggestion
);

public sealed record PublishedRunbookVersionDto(
    Guid RunbookId,
    int VersionNumber,
    string Title,
    string Slug,
    string SerializedAstJson,
    DateTimeOffset PublishedAt
);

public sealed record RunbookAstDto(
    Guid RunbookId,
    int VersionNumber,
    string RootNodeJson,
    IReadOnlyList<RunbookStepNodeDto> Steps
);

public sealed record RunbookStepNodeDto(
    string StepId,
    int StepOrder,
    string StepType, // "Interactive", "AutomatedQuery", "ValidationAssert"
    string Title,
    string CommandScript,
    string ExpectedOutcome,
    IReadOnlyList<string> NextStepIds
);

public sealed record RunbookWorkflowDto(
    Guid WorkflowId,
    Guid DocumentId,
    int VersionNumber,
    string NodesJson, // React Flow nodes: [{ id, type, position: { x, y }, data: { label, ... } }]
    string EdgesJson, // React Flow edges: [{ id, source, target, label?, animated }]
    DateTimeOffset CompiledAt
);
```

> **Workflow compilation errors** (all `ErrorType.Validation`, never thrown): `Workflow.CycleDetected`, `Workflow.UnreachableStep`, `Workflow.InvalidBranchTarget`, `Workflow.InvalidBranchOption`, `Workflow.QuestionWithoutOptions`, `Workflow.DuplicateStepId`, `Workflow.NoSteps`. Branch authoring syntax is owned by [MARKDOWN-PARSER-STANDARDS.md](./standards/MARKDOWN-PARSER-STANDARDS.md#4-workflow-dag-compilation-branching).

---

## 3. Module 2: TicketIngestion (Schema: `ingestion`)

Responsible for syncing incident transcripts, service tickets, and post-mortems from external catalogs—primarily the **read-only F7Hub SQLite database**.

### 3.1 Commands & Queries

```csharp
namespace RunbookForge.BuildingBlocks.Common.Contracts.Ingestion;

// --- Commands ---

public sealed record TriggerF7HubSyncCommand(
    DateTimeOffset? SyncSinceUtc = null,
    int BatchSize = 500
) : ICommand<Result<SyncJobSummaryDto>>;

public sealed record IngestRawTicketBatchCommand(
    string SourceSystem,
    IReadOnlyList<RawTicketInputDto> Tickets
) : ICommand<Result<IngestBatchResultDto>>;

// --- Queries ---

public sealed record GetSyncJobStatusQuery(
    Guid JobId
) : IQuery<Result<SyncJobDetailDto>>;

public sealed record GetRawTicketByIdQuery(
    Guid TicketId
) : IQuery<Result<RawTicketDetailDto>>;
```

### 3.2 Domain Notifications & DTOs

```csharp
// --- Domain Events ---

public sealed record TicketBatchIngestedEvent(
    Guid EventId,
    Guid SyncJobId,
    string SourceCatalog,
    int IngestedTicketCount,
    IReadOnlyList<Guid> IngestedTicketIds,
    DateTimeOffset CompletedAtUtc
) : IDomainEvent;

// --- DTOs ---

public sealed record RawTicketInputDto(
    string ExternalId,
    string Title,
    string Description,
    string ResolutionNotes,
    string Severity,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ResolvedAtUtc
);

public sealed record SyncJobSummaryDto(
    Guid JobId,
    string SourceSystem,
    string Status, // "Pending", "Running", "Completed", "Failed"
    int ProcessedCount,
    DateTimeOffset StartedAtUtc
);

public sealed record IngestBatchResultDto(
    int TotalReceived,
    int SuccessfullyIngested,
    int DuplicatesSkipped,
    IReadOnlyList<Guid> CreatedTicketIds
);
```

---

## 4. Module 3: PatternAnalytics (Schema: `analytics`)

Responsible for ticket symptom clustering, execution duration telemetry, MTTR reduction metrics, and semantic vector embeddings via `pgvector` (with `tsvector` fallback).

### 4.1 Commands & Queries

```csharp
namespace RunbookForge.BuildingBlocks.Common.Contracts.Analytics;

// --- Commands ---

public sealed record GenerateSymptomClustersCommand(
    DateTimeOffset FromDateUtc,
    DateTimeOffset ToDateUtc,
    double SimilarityThreshold = 0.82
) : ICommand<Result<ClusterGenerationResultDto>>;

public sealed record RecordRunbookExecutionTelemetryCommand(
    Guid RunbookId,
    int VersionNumber,
    Guid ExecutionSessionId,
    string TriggerSource,
    TimeSpan TotalDuration,
    bool WasSuccessful,
    IReadOnlyList<StepExecutionTelemetryInputDto> Steps
) : ICommand<Result<ExecutionTelemetryRecordedDto>>;

// --- Queries ---

public sealed record GetSymptomClustersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? UrgencyTier = null
) : IQuery<Result<PagedList<SymptomClusterDto>>>;

public sealed record GetRunbookEfficiencyMetricsQuery(
    Guid RunbookId
) : IQuery<Result<RunbookEfficiencyMetricsDto>>;
```

### 4.2 Domain Notifications & DTOs

```csharp
// --- Domain Events ---

public sealed record SymptomClusterDiscoveredEvent(
    Guid EventId,
    Guid ClusterId,
    string ClusterSignature,
    int TicketFrequency,
    string AffectedSubsystem,
    DateTimeOffset DiscoveredAtUtc
) : IDomainEvent;

// --- DTOs ---

public sealed record StepExecutionTelemetryInputDto(
    string StepId,
    int StepOrder,
    TimeSpan Duration,
    string ExitCode,
    bool Retried
);

public sealed record SymptomClusterDto(
    Guid ClusterId,
    string Title,
    string TopSymptoms,
    int IncidentCount,
    string AssignedUrgency,
    Guid? LinkedRunbookId,
    DateTimeOffset LastIncidentAtUtc
);

public sealed record RunbookEfficiencyMetricsDto(
    Guid RunbookId,
    int TotalExecutions,
    double SuccessRatePercentage,
    TimeSpan AverageExecutionDuration,
    TimeSpan EstimatedMttrReduction
);
```

---

## 5. Module 4: GapAnalyzer (Schema: `gaps`)

Evaluates operational coverage by comparing unmapped symptom clusters with the published runbook catalog, calculating coverage scores and prioritizing drafting targets.

### 5.1 Commands & Queries

```csharp
namespace RunbookForge.BuildingBlocks.Common.Contracts.Gaps;

// --- Commands ---

public sealed record EvaluateOperationalGapsCommand(
    Guid? TriggerClusterId = null
) : ICommand<Result<GapEvaluationReportDto>>;

public sealed record DismissOperationalGapCommand(
    Guid GapId,
    string DismissalReason,
    string DismissedByUserId
) : ICommand<Result>;

// --- Queries ---

public sealed record GetOperationalGapsQuery(
    string? UrgencyFilter = null,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<Result<PagedList<OperationalGapSummaryDto>>>;

public sealed record GetSystemCoverageOverviewQuery() 
    : IQuery<Result<SystemCoverageOverviewDto>>;
```

### 5.2 Domain Notifications & DTOs

```csharp
// --- Domain Events ---

public sealed record OperationalGapIdentifiedEvent(
    Guid EventId,
    Guid GapId,
    Guid SymptomClusterId,
    string Subsystem,
    string UrgencyTier, // "Critical", "High", "Medium", "Low"
    int AffectedIncidentCount,
    DateTimeOffset DetectedAtUtc
) : IDomainEvent;

// --- DTOs ---

public sealed record OperationalGapSummaryDto(
    Guid GapId,
    string Subsystem,
    string UrgencyTier,
    int IncidentFrequency,
    string Description,
    Guid SymptomClusterId,
    DateTimeOffset DetectedAtUtc
);

public sealed record SystemCoverageOverviewDto(
    double GlobalCoveragePercentage,
    int TotalKnownIncidentClusters,
    int CoveredClusters,
    int IdentifiedGapsCount,
    IReadOnlyDictionary<string, double> SubsystemCoverageBreakdown
);
```

---

## 6. Module 5: AICriticAndTraining (Schema: `training`)

Provides AI Critic evaluations of runbook quality, generates synthetic incident injection scenarios, and runs operator training simulations with step scoring.

### 6.1 Commands & Queries

```csharp
namespace RunbookForge.BuildingBlocks.Common.Contracts.Training;

// --- Commands ---

public sealed record RequestAICriticReviewCommand(
    Guid RunbookId,
    int VersionNumber,
    string RawMarkdownContent
) : ICommand<Result<AICriticReviewResultDto>>;

public sealed record StartOperatorDrillCommand(
    Guid ScenarioId,
    string OperatorUserId
) : ICommand<Result<ActiveDrillSessionDto>>;

public sealed record SubmitDrillStepExecutionCommand(
    Guid DrillSessionId,
    string StepId,
    string ExecutedAction,
    TimeSpan StepResponseTime
) : ICommand<Result<DrillStepFeedbackDto>>;

public sealed record FinalizeDrillSessionCommand(
    Guid DrillSessionId
) : ICommand<Result<DrillSessionSummaryDto>>;

// --- Queries ---

public sealed record GetDrillSessionByIdQuery(
    Guid DrillSessionId
) : IQuery<Result<DrillSessionSummaryDto>>;

public sealed record ListAvailablePracticeScenariosQuery(
    Guid? RunbookId = null
) : IQuery<Result<IReadOnlyList<PracticeScenarioSummaryDto>>>;
```

### 6.2 Domain Notifications & DTOs
<a id="critic-dtos"></a>

```csharp
// --- Domain Events ---

public sealed record OperatorDrillCompletedEvent(
    Guid EventId,
    Guid DrillSessionId,
    Guid ScenarioId,
    string OperatorUserId,
    double FinalReadinessScore,
    TimeSpan TotalTimeElapsed,
    DateTimeOffset CompletedAtUtc
) : IDomainEvent;

// --- DTOs ---

public sealed record AICriticReviewResultDto(
    Guid ReviewId,
    double CompletenessScore,
    double ClarityScore,
    double SafetyScore,
    IReadOnlyList<string> Commendations,
    IReadOnlyList<string> Deficiencies,
    IReadOnlyList<string> SuggestedRemediations
);

public sealed record ActiveDrillSessionDto(
    Guid DrillSessionId,
    Guid ScenarioId,
    string ScenarioTitle,
    string InitialIncidentDescription,
    string CurrentStepId,
    DateTimeOffset StartedAtUtc
);

public sealed record DrillStepFeedbackDto(
    Guid DrillSessionId,
    string StepId,
    bool WasActionCorrect,
    string FeedbackMessage,
    string? NextRecommendedStepId
);

public sealed record DrillSessionSummaryDto(
    Guid DrillSessionId,
    Guid ScenarioId,
    string OperatorUserId,
    double OverallScore,
    int TotalStepsCompleted,
    int MistakesCount,
    TimeSpan TotalDuration,
    DateTimeOffset CompletedAtUtc
);
```
