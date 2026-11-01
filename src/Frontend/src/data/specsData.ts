import { SpecDocument } from '../types/controlPlane';

export const SPECS_DATA: SpecDocument[] = [
  {
    id: '00-MASTER-INDEX',
    docPath: 'docs/00-MASTER-INDEX.md',
    title: 'Master Navigation & Context Matrix',
    category: 'System',
    ssotDomain: 'Cross-document dependency graph and slice rules',
    summary: 'The central orientation index mapping subjects to specifications and defining slice limits (3–4 specs max per agent prompt).',
    keyDirectives: [
      'Orient coding agents directly to relevant domain specs without bloating context.',
      'Enforce maximum of 3–4 reference files per /run prompt slice.',
      'Provide topological dependency graph between all 23 documents.'
    ],
    contentSample: `# 00-MASTER-INDEX.md
Orienting AI agents across the RunbookForge workspace (C# .NET 9, React 19, PostgreSQL multi-schema, and F7Hub SQLite).`
  },
  {
    id: '00-ARCHITECTURE',
    docPath: 'docs/00-ARCHITECTURE.md',
    title: 'System Topology & Boundary Rules',
    category: 'System',
    ssotDomain: 'Modular monolith topology, directory layout, and host bootstrapping',
    summary: 'Defines the 5 domain modules, WebApi composition root, assembly references, and strict boundary rules.',
    keyDirectives: [
      'Modular monolith with single WebApi host and 5 isolated domain modules.',
      'Strict zero-cross-module assembly references; inter-module communication is asynchronous via MediatR.',
      'Directory layout: src/BuildingBlocks/, src/Modules/, src/WebApi/, src/Frontend/.'
    ],
    contentSample: `# 00-ARCHITECTURE.md
Modular Monolith System Topology (.NET 9 Minimal APIs, React 19, PostgreSQL, SQLite).`
  },
  {
    id: '01-MODULE-REGISTRY',
    docPath: 'docs/01-MODULE-REGISTRY.md',
    title: 'Module Registry & Public Contracts',
    category: 'System',
    ssotDomain: 'C# MediatR commands, queries, and DTO contracts',
    summary: 'Authoritative registry of public commands, queries, and DTO records across all 5 modules.',
    keyDirectives: [
      'Modules expose public contracts only through Application layer DTOs.',
      'No internal domain entities leak across module boundaries.',
      'Command and Query records implement ICommand<TResult> and IQuery<TResult>.'
    ],
    contentSample: `# 01-MODULE-REGISTRY.md
Public MediatR Command and Query contracts for RunbookEngine, TicketIngestion, PatternAnalytics, GapAnalyzer, and AICriticAndTraining.`
  },
  {
    id: '02-DATA-FLOW-AND-EVENTS',
    docPath: 'docs/02-DATA-FLOW-AND-EVENTS.md',
    title: 'Data Flow & Domain Events',
    category: 'System',
    ssotDomain: 'Inter-module sequence diagrams and event JSON schemas',
    summary: 'Documents cross-module workflows, MediatR event pipeline, and domain event schemas.',
    keyDirectives: [
      'Domain events implement IDomainEvent (INotification).',
      'Event handlers must be idempotent.',
      'Cross-schema notifications trigger asynchronous background workers.'
    ],
    contentSample: `# 02-DATA-FLOW-AND-EVENTS.md
Sequence flows: Ticket Ingestion -> Pattern Discovery -> Gap Analysis -> Runbook Creation -> AI Critic -> Training.`
  },
  {
    id: '03-AI-FEATURE-SOP',
    docPath: 'docs/03-AI-FEATURE-SOP.md',
    title: 'AI Feature Implementation SOP',
    category: 'System',
    ssotDomain: 'Deterministic 6-phase engineering recipe for agents',
    summary: 'Standard operating procedure for Antigravity IDE and Google AI Studio coding agents.',
    keyDirectives: [
      'Phase 1: Pre-Flight Context Inspection.',
      'Phase 2: Scaffolding & Types.',
      'Phase 3: Domain Logic & Handlers.',
      'Phase 4: Persistence & Migrations.',
      'Phase 5: Verification & Link Checks.',
      'Phase 6: Feature State Index Update.'
    ],
    contentSample: `# 03-AI-FEATURE-SOP.md
Step-by-step implementation protocol for autonomous coding agents.`
  },
  {
    id: '04-CONVENTIONS-AND-GUARDRAILS',
    docPath: 'docs/04-CONVENTIONS-AND-GUARDRAILS.md',
    title: 'Conventions & Guardrails',
    category: 'System',
    ssotDomain: 'System-wide invariants, Result<T> pattern, RFC 7807 error codes',
    summary: 'Global non-negotiable architectural rules, Result<T> pattern, and ProblemDetails mappings.',
    keyDirectives: [
      'F7Hub read-only SQLite barrier.',
      'PostgreSQL schema isolation.',
      'Zero business exceptions; return Result<T>.',
      'RFC 7807 ProblemDetails mapping.'
    ],
    contentSample: `# 04-CONVENTIONS-AND-GUARDRAILS.md
Core architectural invariants and coding guardrails.`
  },
  {
    id: '05-FEATURE-STATE-INDEX',
    docPath: 'docs/05-FEATURE-STATE-INDEX.md',
    title: 'Living Feature State Index & Task Tracker',
    category: 'System',
    ssotDomain: 'Feature roadmap, task lifecycle, and active execution status',
    summary: 'Tracks all system tasks (RF-CORE-001 through RF-TRAINING-003) across PLANNED, IN_PROGRESS, and COMPLETED states.',
    keyDirectives: [
      'Maintain status taxonomy: PLANNED, IN_PROGRESS, BLOCKED, COMPLETED.',
      'Single source of truth for task progress.',
      'Contains milestone roadmap M1 through M5.'
    ],
    contentSample: `# 05-FEATURE-STATE-INDEX.md
Living task matrix tracking RunbookForge system progress.`
  },
  {
    id: 'CSHARP-STANDARDS',
    docPath: 'docs/standards/CSHARP-STANDARDS.md',
    title: 'C# .NET 9 & EF Core Standards',
    category: 'Standards',
    ssotDomain: 'C# 13, .NET 9, MediatR pipeline behaviors, and EF Core conventions',
    summary: 'Coding rules for modern C#: primary constructors, sealed classes, xmin concurrency, and zero warnings.',
    keyDirectives: [
      'Target net9.0 with C# 13 and <TreatWarningsAsErrors>true</TreatWarningsAsErrors>.',
      'Use primary constructors for dependency injection.',
      'Map shadow property xmin for optimistic concurrency.'
    ],
    contentSample: `# CSHARP-STANDARDS.md
C# 13 and .NET 9 coding conventions.`
  },
  {
    id: 'TYPESCRIPT-REACT-STANDARDS',
    docPath: 'docs/standards/TYPESCRIPT-REACT-STANDARDS.md',
    title: 'TypeScript & React 19 Standards',
    category: 'Standards',
    ssotDomain: 'React 19, strict TypeScript, TanStack Query, and Tailwind CSS',
    summary: 'Frontend conventions: strict mode, 1:1 type parity with C# DTOs, query key factories, and React Flow.',
    keyDirectives: [
      'React 19 function components with strict TypeScript (zero "any").',
      'TanStack Query v5 key factories for cache invalidation.',
      'Tailwind CSS v4 styling with responsive dark mode.'
    ],
    contentSample: `# TYPESCRIPT-REACT-STANDARDS.md
React 19 and strict TypeScript engineering guidelines.`
  },
  {
    id: 'POSTGRES-SQL-STANDARDS',
    docPath: 'docs/standards/POSTGRES-SQL-STANDARDS.md',
    title: 'PostgreSQL Multi-Schema & Indexing Standards',
    category: 'Standards',
    ssotDomain: 'Schema naming, GIN/HNSW indexing, xmin concurrency, and pooling',
    summary: 'Database guidelines: lower_snake_case, schema isolation, pgvector HNSW, and GIN JSONB indexing.',
    keyDirectives: [
      'Lower_snake_case naming for all tables, columns, and indexes.',
      'GIN indexes on jsonb ast_payload and text tags.',
      'pgvector HNSW cosine distance index with tsvector fallback.'
    ],
    contentSample: `# POSTGRES-SQL-STANDARDS.md
PostgreSQL schema, indexing, and migration rules.`
  },
  {
    id: 'MARKDOWN-PARSER-STANDARDS',
    docPath: 'docs/standards/MARKDOWN-PARSER-STANDARDS.md',
    title: 'Markdig AST & Linter Rules (RF001–RF099)',
    category: 'Standards',
    ssotDomain: 'Markdig pipeline, custom container blocks, and diagnostic engine',
    summary: 'Specifications for :::step and :::assert blocks and RF diagnostic rule taxonomy.',
    keyDirectives: [
      'Custom container parsers for :::step and :::assert blocks.',
      'Deterministic rule codes RF001–RF099.',
      'Non-fatal warnings vs fatal publication blockers.'
    ],
    contentSample: `# MARKDOWN-PARSER-STANDARDS.md
Markdig AST pipeline and diagnostic rule specifications.`
  },
  {
    id: 'TESTING-STANDARDS',
    docPath: 'docs/standards/TESTING-STANDARDS.md',
    title: 'Automated Testing & Quality Gates',
    category: 'Standards',
    ssotDomain: 'xUnit, FluentAssertions, NSubstitute, Testcontainers, and Vitest',
    summary: 'Testing guidelines: MethodName_ShouldExpectedBehavior_WhenStateUnderTest naming, unit isolation, and Testcontainers.',
    keyDirectives: [
      'xUnit + FluentAssertions + NSubstitute for backend unit tests.',
      'Naming convention: MethodName_ShouldExpectedBehavior_WhenStateUnderTest.',
      'Testcontainers PostgreSQL for integration tests.'
    ],
    contentSample: `# TESTING-STANDARDS.md
Automated testing standards and test pyramid structure.`
  },
  {
    id: 'AI-INTEGRATION-STANDARDS',
    docPath: 'docs/standards/AI-INTEGRATION-STANDARDS.md',
    title: 'AI LLM Integration & Structured Rubrics',
    category: 'Standards',
    ssotDomain: 'LLM provider abstraction, structured JSON schema, and token safety',
    summary: 'Architectural rules for Gemini 2.5/Flash and Claude integration with deterministic structured JSON schemas.',
    keyDirectives: [
      'Provider-agnostic IAiClient interface.',
      'Strict JSON schema validation on LLM output.',
      'Prompt templates versioned in code.'
    ],
    contentSample: `# AI-INTEGRATION-STANDARDS.md
LLM integration and structured evaluation rubrics.`
  },
  {
    id: 'DB-00-MASTER-PLAN',
    docPath: 'docs/database/DB-00-MASTER-PLAN.md',
    title: 'PostgreSQL Relational Topology & Master Plan',
    category: 'Database',
    ssotDomain: 'Database topology, isolated schemas, and connection pooling',
    summary: 'Overall PostgreSQL database architecture, schema isolation boundaries, and NpgsqlDataSource configuration.',
    keyDirectives: [
      'Single PostgreSQL database with 5 isolated schemas: runbooks, ingestion, analytics, gaps, training.',
      'Shared NpgsqlDataSource connection pooling.',
      'Zero cross-schema foreign keys.'
    ],
    contentSample: `# DB-00-MASTER-PLAN.md
PostgreSQL topology and connection pooling plan.`
  },
  {
    id: 'DB-01-SCHEMA-RUNBOOKS',
    docPath: 'docs/database/DB-01-SCHEMA-RUNBOOKS.md',
    title: 'Schema: runbooks (RunbookEngine)',
    category: 'Database',
    ssotDomain: 'DDL for documents, versions, lint_results, workflows',
    summary: 'Complete DDL specifications for the runbooks schema with GIN AST indexes and optimistic concurrency.',
    keyDirectives: [
      'Tables: documents, versions, lint_results, workflows.',
      'GIN index on versions.ast_payload (jsonb_path_ops).',
      'Concurrency token: xmin uint.'
    ],
    contentSample: `# DB-01-SCHEMA-RUNBOOKS.md
DDL and EF Core configuration for schema runbooks.`
  },
  {
    id: 'DB-02-SCHEMA-INGESTION',
    docPath: 'docs/database/DB-02-SCHEMA-INGESTION.md',
    title: 'Schema: ingestion (TicketIngestion)',
    category: 'Database',
    ssotDomain: 'DDL for sync_jobs, raw_tickets, ticket_hashes',
    summary: 'DDL specifications for sync jobs, raw ticket snapshots, and SHA256 deduplication hashes.',
    keyDirectives: [
      'Tables: sync_jobs, raw_tickets, ticket_hashes.',
      'Unique index on ticket_hashes(content_hash).',
      'Watermark tracking for incremental sync.'
    ],
    contentSample: `# DB-02-SCHEMA-INGESTION.md
DDL specifications for schema ingestion.`
  },
  {
    id: 'DB-03-SCHEMA-ANALYTICS',
    docPath: 'docs/database/DB-03-SCHEMA-ANALYTICS.md',
    title: 'Schema: analytics (PatternAnalytics)',
    category: 'Database',
    ssotDomain: 'DDL for symptom_clusters, ticket_embeddings, telemetry',
    summary: 'DDL specifications for pgvector 1536-dim embeddings, HNSW index, and tsvector full-text fallback.',
    keyDirectives: [
      'Tables: symptom_clusters, ticket_embeddings, telemetry_traces.',
      'HNSW index on embedding vector(1536) using vector_cosine_ops.',
      'GIN index on search_vector (tsvector) fallback.'
    ],
    contentSample: `# DB-03-SCHEMA-ANALYTICS.md
DDL specifications for schema analytics with vector search.`
  },
  {
    id: 'DB-04-SCHEMA-GAPS',
    docPath: 'docs/database/DB-04-SCHEMA-GAPS.md',
    title: 'Schema: gaps (GapAnalyzer)',
    category: 'Database',
    ssotDomain: 'DDL for coverage_reports, missing_patterns',
    summary: 'DDL specifications for subsystem coverage reports and prioritized missing pattern logs.',
    keyDirectives: [
      'Tables: coverage_reports, missing_patterns, gap_evaluations.',
      'Severity and frequency weighting columns.'
    ],
    contentSample: `# DB-04-SCHEMA-GAPS.md
DDL specifications for schema gaps.`
  },
  {
    id: 'DB-05-SCHEMA-TRAINING',
    docPath: 'docs/database/DB-05-SCHEMA-TRAINING.md',
    title: 'Schema: training (AICriticAndTraining)',
    category: 'Database',
    ssotDomain: 'DDL for practice_scenarios, evaluation_logs',
    summary: 'DDL specifications for AI rubric evaluations and simulated operator drill sessions.',
    keyDirectives: [
      'Tables: practice_scenarios, evaluation_logs, drill_sessions.',
      'JSONB storage for rubric criteria breakdowns.'
    ],
    contentSample: `# DB-05-SCHEMA-TRAINING.md
DDL specifications for schema training.`
  },
  {
    id: 'DB-06-EXTERNAL-F7HUB-READONLY',
    docPath: 'docs/database/DB-06-EXTERNAL-F7HUB-READONLY.md',
    title: 'External F7Hub SQLite Read-Only Security Barrier',
    category: 'Database',
    ssotDomain: 'Read-only adapter rules, connection flags, and DbContext security barrier',
    summary: 'Strict security specifications for read-only access to f7hub.db SQLite database.',
    keyDirectives: [
      'Connection string: Data Source=f7hub.db;Mode=ReadOnly;Cache=Shared;.',
      'Global QueryTrackingBehavior.NoTracking.',
      'SaveChanges and SaveChangesAsync must throw InvalidOperationException.',
      'Zero EF migrations or schema alterations allowed on F7Hub.'
    ],
    contentSample: `# DB-06-EXTERNAL-F7HUB-READONLY.md
Absolute read-only security barrier specifications for F7Hub SQLite database.`
  }
];

