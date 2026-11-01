import { ModuleTopology } from '../types/controlPlane';

export const MODULE_TOPOLOGY_DATA: ModuleTopology[] = [
  {
    id: 'RunbookEngine',
    displayName: 'Runbook Engine',
    schema: 'runbooks',
    dbContext: 'RunbookDbContext',
    primaryTables: ['documents', 'versions', 'lint_results', 'workflows'],
    inboundEvents: ['SymptomClusterDiscoveredEvent'],
    outboundEvents: ['RunbookVersionPublishedEvent', 'RunbookArchivedEvent'],
    publicCommands: ['CreateRunbookDraftCommand', 'UpdateRunbookDraftCommand', 'PublishRunbookVersionCommand'],
    publicQueries: ['GetRunbookBySlugQuery', 'LintRunbookQuery', 'GetPublishedRunbooksQuery'],
    description: 'Markdown AST parser, step editor, diagnostic linter (RF001–RF099), and version publication.'
  },
  {
    id: 'TicketIngestion',
    displayName: 'Ticket Ingestion',
    schema: 'ingestion',
    dbContext: 'IngestionDbContext',
    primaryTables: ['sync_jobs', 'raw_tickets', 'ticket_hashes'],
    inboundEvents: [],
    outboundEvents: ['TicketBatchIngestedEvent'],
    publicCommands: ['TriggerIncidentSyncCommand', 'ResetWatermarkCommand'],
    publicQueries: ['GetSyncJobStatusQuery', 'GetIngestionMetricsQuery'],
    readOnlyBoundary: true,
    description: 'Scheduled incident synchronization worker, SHA256 content deduplication, and read-only F7Hub SQLite security adapter.'
  },
  {
    id: 'PatternAnalytics',
    displayName: 'Pattern Analytics',
    schema: 'analytics',
    dbContext: 'AnalyticsDbContext',
    primaryTables: ['symptom_clusters', 'ticket_embeddings', 'telemetry_traces'],
    inboundEvents: ['TicketBatchIngestedEvent', 'RunbookVersionPublishedEvent'],
    outboundEvents: ['SymptomClusterDiscoveredEvent', 'PatternFrequencyShiftedEvent'],
    publicCommands: ['GenerateClusterEmbeddingsCommand', 'RecordExecutionTelemetryCommand'],
    publicQueries: ['GetClusterDetailsQuery', 'GetRunbookMttrStatsQuery'],
    description: 'Vector embeddings with pgvector HNSW indexing, full-text tsvector fallback, symptom clustering, and MTTR telemetries.'
  },
  {
    id: 'GapAnalyzer',
    displayName: 'Gap Analyzer',
    schema: 'gaps',
    dbContext: 'GapDbContext',
    primaryTables: ['coverage_reports', 'missing_patterns', 'gap_evaluations'],
    inboundEvents: ['SymptomClusterDiscoveredEvent', 'RunbookVersionPublishedEvent'],
    outboundEvents: ['CoverageGapDetectedEvent', 'GapResolvedEvent'],
    publicCommands: ['EvaluateCoverageGapsCommand'],
    publicQueries: ['GetCoverageOverviewQuery', 'GetSubsystemGapsQuery'],
    description: 'Operational coverage matrix, missing runbook detector, and incident pattern gap prioritization engine.'
  },
  {
    id: 'AICriticAndTraining',
    displayName: 'AI Critic & Training',
    schema: 'training',
    dbContext: 'TrainingDbContext',
    primaryTables: ['practice_scenarios', 'evaluation_logs', 'drill_sessions'],
    inboundEvents: ['RunbookVersionPublishedEvent'],
    outboundEvents: ['RunbookCriticEvaluationCompletedEvent', 'DrillSessionStartedEvent'],
    publicCommands: ['EvaluateRunbookWithAiCommand', 'StartOperatorDrillCommand', 'SubmitDrillStepCommand'],
    publicQueries: ['GetEvaluationReportQuery', 'GetDrillSessionQuery'],
    description: 'LLM rubric evaluation (completeness, clarity, safety), automated practice scenario generation, and operator incident drill simulator.'
  }
];

