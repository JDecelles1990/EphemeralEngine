import { TaskDefinition } from '../types/controlPlane';

export const TASKS_DATA: TaskDefinition[] = [
  {
    id: 'RF-CORE-001',
    title: 'Scaffold Solution Directory Layout & .csproj References',
    module: 'Core',
    schema: 'none',
    layer: 'BuildingBlocks',
    status: 'COMPLETED',
    contextSpecs: [
      'docs/00-ARCHITECTURE.md',
      'docs/04-CONVENTIONS-AND-GUARDRAILS.md',
      'docs/standards/CSHARP-STANDARDS.md'
    ],
    objective:
      'Scaffold the complete .NET 9 solution hierarchy, compile-time guardrail properties, shared BuildingBlocks, isolated project shells for all five modules, the ASP.NET Core WebApi host, and the Vite + React 19 frontend foundation.',
    sequence: [
      {
        step: 1,
        title: 'Root & Build Configuration',
        details: [
          'Create Directory.Build.props with net9.0, Nullable=enable, TreatWarningsAsErrors=true, ImplicitUsings=enable.',
          'Create RunbookForge.sln registering all projects.'
        ]
      },
      {
        step: 2,
        title: 'Shared BuildingBlocks Scaffolding',
        details: [
          'BuildingBlocks.Common: Result<T>, Error, ErrorType, ICommand, IQuery, IDomainEvent, LoggingBehavior, ValidationBehavior.',
          'BuildingBlocks.Persistence: SchemaConstants (runbooks, ingestion, analytics, gaps, training).',
          'BuildingBlocks.Web: IEndpointModule, ResultExtensions (ProblemDetails).'
        ]
      },
      {
        step: 3,
        title: 'Isolated Module Shells & WebApi Root',
        details: [
          '5 isolated modules (Domain, Application, Infrastructure) with strictly zero cross-module references.',
          'WebApi Minimal API composition root with MediatR assembly scanning, Swagger, and health checks.'
        ]
      },
      {
        step: 4,
        title: 'Frontend Vite & React 19 Foundation',
        details: [
          'Initialize Vite + React 19 + TypeScript strict mode with Tailwind CSS v4 and TanStack Query.'
        ]
      }
    ],
    verification: [
      { description: 'Verify .NET build succeeds with zero errors and warnings', command: 'dotnet build RunbookForge.sln' },
      { description: 'Verify Frontend production bundle compiles cleanly', command: 'npm --prefix src/Frontend run build' },
      { description: 'Verify internal markdown documentation link integrity', command: 'pwsh ./scripts/Test-MarkdownLinks.ps1 -DocsRoot "docs"' }
    ],
    dependencies: [],
    notes: 'Verified: 19 .NET projects build with 0 warnings/errors, Vite frontend builds with 0 type errors.'
  },
  {
    id: 'RF-CORE-002',
    title: 'Unit Test Suite for Shared BuildingBlocks',
    module: 'Core',
    schema: 'none',
    layer: 'BuildingBlocks.Common',
    status: 'COMPLETED',
    contextSpecs: [
      'docs/03-AI-FEATURE-SOP.md',
      'docs/04-CONVENTIONS-AND-GUARDRAILS.md',
      'docs/standards/TESTING-STANDARDS.md',
      'docs/standards/CSHARP-STANDARDS.md'
    ],
    objective:
      'Create and execute the test project tests/BuildingBlocks/BuildingBlocks.Common.Tests/ using xUnit, FluentAssertions, and NSubstitute to achieve complete test coverage for Result, Result<T>, Error, ValidationBehavior, and ResultExtensions.',
    sequence: [
      {
        step: 1,
        title: 'Test Project Scaffolding',
        details: [
          'Create tests/BuildingBlocks/BuildingBlocks.Common.Tests/BuildingBlocks.Common.Tests.csproj targeting net9.0.',
          'Reference xunit, FluentAssertions, NSubstitute, Microsoft.NET.Test.Sdk.',
          'Reference RunbookForge.BuildingBlocks.Common and RunbookForge.BuildingBlocks.Web.'
        ]
      },
      {
        step: 2,
        title: 'Domain Primitives Test Suite (ResultTests.cs)',
        details: [
          'Verify Result.Success() sets IsSuccess = true and Error = Error.None.',
          'Verify Result.Failure(error) preserves payload and sets IsFailure = true.',
          'Verify Result<T>.Value throws InvalidOperationException when accessed on failure.',
          'Verify implicit conversion T -> Result<T> handles non-null and null fallbacks.'
        ]
      },
      {
        step: 3,
        title: 'MediatR Pipeline Behavior Test Suite (ValidationBehaviorTests.cs)',
        details: [
          'Verify handlers execute normally when validators pass or are omitted.',
          'Verify pipeline short-circuits and returns Result.Failure(Error.Validation(...)) without throwing exceptions.',
          'Verify generic Result<T> failure creation via reflection on validation errors.'
        ]
      },
      {
        step: 4,
        title: 'ProblemDetails Mapping Test Suite (ResultExtensionsTests.cs)',
        details: [
          'Verify mapping ErrorType.Validation produces HTTP 400 ProblemDetails.',
          'Verify mapping ErrorType.NotFound produces HTTP 404 ProblemDetails.',
          'Verify mapping ErrorType.Conflict produces HTTP 409 ProblemDetails.',
          'Verify errorCode extension dictionary entry preservation.'
        ]
      }
    ],
    verification: [
      { description: 'Run complete unit test suite', command: 'dotnet test tests/BuildingBlocks/BuildingBlocks.Common.Tests/' },
      { description: 'Verify clean solution build', command: 'dotnet build RunbookForge.sln' },
      { description: 'Check documentation state', command: 'pwsh ./scripts/Test-MarkdownLinks.ps1 -DocsRoot "docs"' }
    ],
    dependencies: ['RF-CORE-001'],
    notes: 'Verified: 30 xUnit tests passed (Result, Behaviors, RFC 7807 mappings).'
  },
  {
    id: 'RF-CORE-003',
    title: 'MediatR Pipeline Behaviors (Logging, Validation, Transaction)',
    module: 'Core',
    schema: 'none',
    layer: 'BuildingBlocks.Common',
    status: 'COMPLETED',
    contextSpecs: [
      'docs/02-DATA-FLOW-AND-EVENTS.md',
      'docs/standards/CSHARP-STANDARDS.md',
      'docs/04-CONVENTIONS-AND-GUARDRAILS.md'
    ],
    objective:
      'Ensure standard MediatR pipeline behaviors for structured Serilog correlation IDs, non-throwing FluentValidation, and unit test instrumentation.',
    sequence: [
      {
        step: 1,
        title: 'Logging Pipeline Behavior',
        details: [
          'Implement LoggingBehavior measuring execution latency with Stopwatch.',
          'Log incoming request names and execution elapsed milliseconds.'
        ]
      },
      {
        step: 2,
        title: 'Validation Pipeline Behavior',
        details: [
          'Implement non-throwing ValidationBehavior aggregating FluentValidation failures.',
          'Return Result.Failure(Error.Validation(...)) with property breakdown.'
        ]
      }
    ],
    verification: [
      { description: 'Verify behavior unit test execution', command: 'dotnet test tests/BuildingBlocks/BuildingBlocks.Common.Tests/ --filter "FullyQualifiedName~Behaviors"' }
    ],
    dependencies: ['RF-CORE-001'],
    notes: 'Embedded with FluentValidation & LoggingBehavior unit test coverage.'
  },
  {
    id: 'RF-CORE-004',
    title: 'PostgreSQL Connection Pooling & NpgsqlDataSource Setup',
    module: 'Core',
    schema: 'none',
    layer: 'BuildingBlocks.Persistence',
    status: 'PLANNED',
    contextSpecs: [
      'docs/database/DB-00-MASTER-PLAN.md',
      'docs/standards/POSTGRES-SQL-STANDARDS.md'
    ],
    objective:
      'Configure centralized NpgsqlDataSource pooling with isolated schema search path routing and pgvector extension mapping.',
    sequence: [
      {
        step: 1,
        title: 'NpgsqlDataSourceBuilder Extension',
        details: [
          'Configure connection string parsing with min/max pool size constraints.',
          'Register pgvector type mapping when vector extension is present.'
        ]
      },
      {
        step: 2,
        title: 'Schema Search Path Middleware',
        details: [
          'Enforce default schema isolation per DbContext registration.'
        ]
      }
    ],
    verification: [
      { description: 'Verify persistence assembly build', command: 'dotnet build src/BuildingBlocks/Persistence/RunbookForge.BuildingBlocks.Persistence.csproj' }
    ],
    dependencies: ['RF-CORE-001']
  },
  {
    id: 'RF-CORE-005',
    title: 'WebApi Composition Root & Minimal API Dynamic Discovery',
    module: 'Core',
    schema: 'none',
    layer: 'WebApi',
    status: 'COMPLETED',
    contextSpecs: [
      'docs/00-ARCHITECTURE.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Dynamically register all IEndpointModule implementations and configure Swagger OpenAPI documentation and health checks.',
    sequence: [
      {
        step: 1,
        title: 'Endpoint Module Scanner',
        details: [
          'Scan loaded module assemblies for IEndpointModule implementations.',
          'Map endpoints dynamically on WebApplication startup.'
        ]
      },
      {
        step: 2,
        title: 'Health Check & Swagger Integration',
        details: [
          'Register /health probe and Swagger UI with RFC 7807 ProblemDetails documentation.'
        ]
      }
    ],
    verification: [
      { description: 'Verify WebApi build', command: 'dotnet build src/WebApi/RunbookForge.WebApi.csproj' }
    ],
    dependencies: ['RF-CORE-001'],
    notes: 'Dynamic IEndpointModule discovery, Swagger, and health checks initialized.'
  },
  {
    id: 'RF-RUNBOOK-001',
    title: 'runbooks PostgreSQL Schema Migrations & EF Core DbContext',
    module: 'RunbookEngine',
    schema: 'runbooks',
    layer: 'RunbookEngine.Infrastructure',
    status: 'COMPLETED',
    contextSpecs: [
      'docs/database/DB-01-SCHEMA-RUNBOOKS.md',
      'docs/standards/POSTGRES-SQL-STANDARDS.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Implement RunbookDbContext and EF Core migrations for the runbooks schema, mapping documents, versions, lint_results, and workflows with xmin concurrency tokens.',
    sequence: [
      {
        step: 1,
        title: 'Domain Entities',
        details: [
          'Create RunbookDocument, RunbookVersion, LintResultRecord, and WorkflowStep entities.',
          'Enforce uint xmin shadow property for optimistic concurrency.'
        ]
      },
      {
        step: 2,
        title: 'DbContext & Fluent Configurations',
        details: [
          'Implement RunbookDbContext with HasDefaultSchema("runbooks").',
          'Configure GIN indexes on jsonb ast_payload and tags.'
        ]
      },
      {
        step: 3,
        title: 'EF Core Migration Generation',
        details: [
          'Generate InitialRunbooksSchema migration with idempotent DDL script.'
        ]
      }
    ],
    verification: [
      { description: 'Verify RunbookEngine build', command: 'dotnet build src/Modules/RunbookEngine/Infrastructure/RunbookForge.Modules.RunbookEngine.Infrastructure.csproj' },
      { description: 'Run migration bundle validation', command: 'dotnet ef migrations list --project src/Modules/RunbookEngine/Infrastructure' }
    ],
    dependencies: ['RF-CORE-001', 'RF-CORE-002'],
    notes: 'Verified: RunbookDbContext, 4 entities, EF Core migration Initial_Runbooks_Schema with xmin concurrency & GIN indexes.'
  },
  {
    id: 'RF-RUNBOOK-002',
    title: 'Markdig AST Pipeline & Custom Container Block Parsers',
    module: 'RunbookEngine',
    schema: 'runbooks',
    layer: 'RunbookEngine.Infrastructure',
    status: 'COMPLETED',
    contextSpecs: [
      'docs/standards/MARKDOWN-PARSER-STANDARDS.md',
      'docs/standards/CSHARP-STANDARDS.md'
    ],
    objective:
      'Build custom Markdig MarkdownPipeline extensions to parse :::step, :::assert, and executable code fences into structured C# AST models.',
    sequence: [
      {
        step: 1,
        title: 'Custom Block Parsers',
        details: [
          'Implement StepBlockParser and AssertBlockParser inheriting BlockParser.',
          'Implement StepBlockRenderer producing JSON AST representations.'
        ]
      },
      {
        step: 2,
        title: 'Executable Fence Inspector',
        details: [
          'Extract target shell, script payload, timeout, and expected exit codes.'
        ]
      }
    ],
    verification: [
      { description: 'Execute Markdig parser unit tests', command: 'dotnet test tests/Modules/RunbookEngine.Tests/' }
    ],
    dependencies: ['RF-RUNBOOK-001'],
    notes: 'Verified: Custom Markdig AST container parsers, nested block handling, code fence attributes, 7/7 xUnit tests passed.'
  },
  {
    id: 'RF-RUNBOOK-003',
    title: 'Diagnostic Linter Engine (RF001–RF099 Rules)',
    module: 'RunbookEngine',
    schema: 'runbooks',
    layer: 'RunbookEngine.Application',
    status: 'IN_PROGRESS',
    contextSpecs: [
      'docs/standards/MARKDOWN-PARSER-STANDARDS.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Implement the deterministic diagnostic rule engine verifying YAML frontmatter (RF001), step sequences (RF010), assert commands (RF020), and timeout bounds (RF030).',
    sequence: [
      {
        step: 1,
        title: 'Linter Rule Registry',
        details: [
          'Implement IRuleDiagnosticProvider interface returning DiagnosticItem records.',
          'Add FrontmatterRequiredRule (RF001) and StepSequenceRule (RF010).'
        ]
      },
      {
        step: 2,
        title: 'Linter Aggregation Handler',
        details: [
          'Implement LintRunbookQueryHandler returning LintResultDto with error/warning counts.'
        ]
      }
    ],
    verification: [
      { description: 'Run linter test cases', command: 'dotnet test tests/Modules/RunbookEngine.Tests/ --filter "FullyQualifiedName~Rules"' }
    ],
    dependencies: ['RF-RUNBOOK-002'],
    notes: 'Batch A complete: Core contracts, RunbookLinterEngine, LintRunbookQuery/Handler, and rules RF001–RF015 verified with 82 unit tests.'
  },
  {
    id: 'RF-RUNBOOK-004',
    title: 'AST to Workflow DAG Compiler & React Flow Schema Serialization',
    module: 'RunbookEngine',
    schema: 'runbooks',
    layer: 'RunbookEngine.Domain',
    status: 'COMPLETED',
    contextSpecs: [
      'docs/standards/MARKDOWN-PARSER-STANDARDS.md',
      'docs/standards/CSHARP-STANDARDS.md',
      'docs/database/DB-01-SCHEMA-RUNBOOKS.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Transform the parsed RunbookAstDocument into an executable DAG with 3-colour DFS cycle detection, Kahn ranking, lane layout, React Flow nodes_json/edges_json serialization, and persistence into runbooks.workflows via IRunbookDbContext.',
    sequence: [
      {
        step: 1,
        title: 'Domain Graph Models & DAG Compiler',
        details: [
          'WorkflowNodeType, NodePosition, WorkflowNode, WorkflowEdge, WorkflowGraph records.',
          'WorkflowDagCompiler: Start/Step/Decision/Escalation/Terminal nodes, :::question branching (Label->step-id), Y = depth * 150, X = 250 + (lane - center) * 280.',
          'Cycle, unreachable-step, duplicate-id and invalid-branch failures returned as Error.Validation results.'
        ]
      },
      {
        step: 2,
        title: 'React Flow Serialization & Persistence Handler',
        details: [
          'ReactFlowSerializer emits { id, type, position: { x, y }, data } nodes and { id, source, target, label, animated } edges.',
          'CompileRunbookWorkflowHandler upserts the runbooks.workflows row through IRunbookDbContext.'
        ]
      }
    ],
    verification: [
      { description: 'Run workflow compiler tests', command: 'dotnet test tests/Modules/RunbookEngine.Tests/ --filter FullyQualifiedName~Workflow' },
      { description: 'Verify solution build', command: 'dotnet build RunbookForge.sln' }
    ],
    dependencies: ['RF-RUNBOOK-001', 'RF-RUNBOOK-002'],
    notes: 'Verified: 36 workflow tests passed; solution builds with 0 warnings/errors.'
  },
  {
    id: 'RF-RUNBOOK-005',
    title: 'React Flow Step Graph Visualizer & Markdown Editor UI',
    module: 'RunbookEngine',
    schema: 'runbooks',
    layer: 'Frontend',
    status: 'PLANNED',
    contextSpecs: [
      'docs/standards/TYPESCRIPT-REACT-STANDARDS.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Build the interactive React Flow visualizer for executable step DAGs with inline markdown editing and real-time lint error highlighting.',
    sequence: [
      {
        step: 1,
        title: 'React Flow Custom Nodes',
        details: [
          'Implement StepNode, AssertNode, and ExecutionOutputNode components.',
          'Render execution status and duration badges.'
        ]
      },
      {
        step: 2,
        title: 'Markdown AST Synchronizer',
        details: [
          'Two-way sync between raw markdown buffer and React Flow graph nodes.',
          'Display RF diagnostic squiggles in the editor gutter.'
        ]
      }
    ],
    verification: [
      { description: 'Run frontend component tests', command: 'npm --prefix src/Frontend run test' }
    ],
    dependencies: ['RF-RUNBOOK-004']
  },
  {
    id: 'RF-RUNBOOK-006',
    title: 'Runbook Draft & Version Publication MediatR Handlers',
    module: 'RunbookEngine',
    schema: 'runbooks',
    layer: 'RunbookEngine.Application',
    status: 'PLANNED',
    contextSpecs: [
      'docs/01-MODULE-REGISTRY.md',
      'docs/02-DATA-FLOW-AND-EVENTS.md',
      'docs/03-AI-FEATURE-SOP.md'
    ],
    objective:
      'Implement CreateRunbookDraftCommand, UpdateRunbookDraftCommand, and PublishRunbookVersionCommand handlers with optimistic concurrency and domain event dispatch.',
    sequence: [
      {
        step: 1,
        title: 'Draft Command Handlers',
        details: [
          'Validate slug uniqueness and frontmatter schema.',
          'Save new draft revision with incremented minor version number.'
        ]
      },
      {
        step: 2,
        title: 'Publication Handler & Event Dispatch',
        details: [
          'Verify zero fatal linter diagnostics before allowing publication.',
          'Dispatch RunbookVersionPublishedEvent to MediatR notification handlers.'
        ]
      }
    ],
    verification: [
      { description: 'Verify handler CQRS test suite', command: 'dotnet test tests/Modules/RunbookEngine.Tests/ --filter "Category=Commands"' }
    ],
    dependencies: ['RF-RUNBOOK-001', 'RF-RUNBOOK-003'],
    notes: 'Renumbered from the former RF-RUNBOOK-004; ID 004 now identifies the Workflow DAG Compiler.'
  },
  {
    id: 'RF-INGEST-001',
    title: 'ingestion PostgreSQL Schema Migrations & DbContext',
    module: 'TicketIngestion',
    schema: 'ingestion',
    layer: 'TicketIngestion.Infrastructure',
    status: 'PLANNED',
    contextSpecs: [
      'docs/database/DB-02-SCHEMA-INGESTION.md',
      'docs/standards/POSTGRES-SQL-STANDARDS.md'
    ],
    objective:
      'Configure IngestionDbContext and migrations for sync_jobs, raw_tickets, and ticket_hashes in schema ingestion.',
    sequence: [
      {
        step: 1,
        title: 'Entities & Fluent Mapping',
        details: [
          'Define SyncJob, RawTicket, and TicketHash entities with SHA256 unique index.',
          'Map schema to ingestion with xmin concurrency.'
        ]
      }
    ],
    verification: [
      { description: 'Build TicketIngestion infrastructure', command: 'dotnet build src/Modules/TicketIngestion/Infrastructure' }
    ],
    dependencies: ['RF-CORE-001']
  },
  {
    id: 'RF-INGEST-002',
    title: 'Read-Only F7Hub SQLite Adapter (F7HubReadOnlyDbContext)',
    module: 'TicketIngestion',
    schema: 'external_sqlite',
    layer: 'TicketIngestion.Infrastructure',
    status: 'PLANNED',
    contextSpecs: [
      'docs/database/DB-06-EXTERNAL-F7HUB-READONLY.md',
      'docs/04-CONVENTIONS-AND-GUARDRAILS.md'
    ],
    objective:
      'Implement the absolute read-only security barrier connecting to f7hub.db using Mode=ReadOnly;Cache=Shared; and throwing InvalidOperationException on any write attempts.',
    sequence: [
      {
        step: 1,
        title: 'F7HubReadOnlyDbContext Implementation',
        details: [
          'Configure SQLite connection with Mode=ReadOnly;Cache=Shared;.',
          'Override SaveChanges and SaveChangesAsync to throw InvalidOperationException.',
          'Set ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking.'
        ]
      },
      {
        step: 2,
        title: 'Security Barrier Unit Tests',
        details: [
          'Verify attempts to insert, update, or delete throw InvalidOperationException.',
          'Verify connection string rejects ReadWrite modes.'
        ]
      }
    ],
    verification: [
      { description: 'Execute F7Hub security barrier tests', command: 'dotnet test tests/Modules/TicketIngestion.Tests/ --filter "Category=F7HubSecurity"' }
    ],
    dependencies: ['RF-CORE-001']
  },
  {
    id: 'RF-INGEST-003',
    title: 'Incident Sync Worker & Content Deduplication Pipeline',
    module: 'TicketIngestion',
    schema: 'ingestion',
    layer: 'TicketIngestion.Application',
    status: 'PLANNED',
    contextSpecs: [
      'docs/01-MODULE-REGISTRY.md',
      'docs/02-DATA-FLOW-AND-EVENTS.md'
    ],
    objective:
      'Build the periodic background sync worker that extracts incidents from F7Hub SQLite, computes SHA256 hashes for deduplication, and publishes TicketBatchIngestedEvent.',
    sequence: [
      {
        step: 1,
        title: 'Sync Worker Implementation',
        details: [
          'Query F7Hub tickets incrementally using watermark timestamp.',
          'Calculate SHA256 hash across title, body, and symptoms.'
        ]
      },
      {
        step: 2,
        title: 'Event Publishing',
        details: [
          'Publish TicketBatchIngestedEvent via MediatR with batch ID and count.'
        ]
      }
    ],
    verification: [
      { description: 'Execute sync worker unit tests', command: 'dotnet test tests/Modules/TicketIngestion.Tests/ --filter "Category=SyncWorker"' }
    ],
    dependencies: ['RF-INGEST-001', 'RF-INGEST-002']
  },
  {
    id: 'RF-ANALYTICS-001',
    title: 'analytics PostgreSQL Schema Migrations & Vector Setup',
    module: 'PatternAnalytics',
    schema: 'analytics',
    layer: 'PatternAnalytics.Infrastructure',
    status: 'PLANNED',
    contextSpecs: [
      'docs/database/DB-03-SCHEMA-ANALYTICS.md',
      'docs/standards/POSTGRES-SQL-STANDARDS.md'
    ],
    objective:
      'Configure AnalyticsDbContext with pgvector HNSW indexing and automatic fallback to PostgreSQL tsvector full-text search.',
    sequence: [
      {
        step: 1,
        title: 'Schema & Vector Mapping',
        details: [
          'Create symptom_clusters and ticket_embeddings tables.',
          'Add pgvector 1536-dim column with HNSW cosine distance index.',
          'Add tsvector generated column with GIN index for search fallback.'
        ]
      }
    ],
    verification: [
      { description: 'Build PatternAnalytics infrastructure', command: 'dotnet build src/Modules/PatternAnalytics/Infrastructure' }
    ],
    dependencies: ['RF-CORE-001', 'RF-CORE-004']
  },
  {
    id: 'RF-ANALYTICS-002',
    title: 'Ticket Symptom Clustering & Embedding Generation Engine',
    module: 'PatternAnalytics',
    schema: 'analytics',
    layer: 'PatternAnalytics.Application',
    status: 'PLANNED',
    contextSpecs: [
      'docs/standards/AI-INTEGRATION-STANDARDS.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Consume TicketBatchIngestedEvent, generate dense embeddings, cluster symptoms via HDBSCAN/Cosine threshold, and publish SymptomClusterDiscoveredEvent.',
    sequence: [
      {
        step: 1,
        title: 'Embedding Consumer Handler',
        details: [
          'Call LLM embedding provider with batch tokenization.',
          'Store vector in ticket_embeddings table.'
        ]
      },
      {
        step: 2,
        title: 'Clustering Engine & Notification',
        details: [
          'Cluster nearby symptom vectors and emit SymptomClusterDiscoveredEvent.'
        ]
      }
    ],
    verification: [
      { description: 'Run clustering unit tests', command: 'dotnet test tests/Modules/PatternAnalytics.Tests/' }
    ],
    dependencies: ['RF-ANALYTICS-001']
  },
  {
    id: 'RF-ANALYTICS-003',
    title: 'Runbook Execution Telemetry & MTTR Calculation Handlers',
    module: 'PatternAnalytics',
    schema: 'analytics',
    layer: 'PatternAnalytics.Application',
    status: 'PLANNED',
    contextSpecs: [
      'docs/01-MODULE-REGISTRY.md',
      'docs/02-DATA-FLOW-AND-EVENTS.md'
    ],
    objective:
      'Record execution telemetry and calculate MTTR reductions per runbook and incident cluster.',
    sequence: [
      {
        step: 1,
        title: 'Telemetry Ingestion Handler',
        details: [
          'Handle RecordExecutionTelemetryCommand recording step timings and exit codes.'
        ]
      }
    ],
    verification: [
      { description: 'Verify telemetry tests', command: 'dotnet test tests/Modules/PatternAnalytics.Tests/ --filter "Category=Telemetry"' }
    ],
    dependencies: ['RF-ANALYTICS-001']
  },
  {
    id: 'RF-GAPS-001',
    title: 'gaps PostgreSQL Schema Migrations & DbContext',
    module: 'GapAnalyzer',
    schema: 'gaps',
    layer: 'GapAnalyzer.Infrastructure',
    status: 'PLANNED',
    contextSpecs: [
      'docs/database/DB-04-SCHEMA-GAPS.md',
      'docs/standards/POSTGRES-SQL-STANDARDS.md'
    ],
    objective:
      'Configure GapDbContext mapping coverage_reports and missing_patterns in schema gaps.',
    sequence: [
      {
        step: 1,
        title: 'Gap Entities & Schema Definition',
        details: [
          'Create CoverageReport and MissingPattern entities with severity and impact weights.'
        ]
      }
    ],
    verification: [
      { description: 'Build GapAnalyzer infrastructure', command: 'dotnet build src/Modules/GapAnalyzer/Infrastructure' }
    ],
    dependencies: ['RF-CORE-001']
  },
  {
    id: 'RF-GAPS-002',
    title: 'Coverage Evaluation Engine (Cluster vs Runbook Mapping)',
    module: 'GapAnalyzer',
    schema: 'gaps',
    layer: 'GapAnalyzer.Application',
    status: 'PLANNED',
    contextSpecs: [
      'docs/01-MODULE-REGISTRY.md',
      'docs/02-DATA-FLOW-AND-EVENTS.md'
    ],
    objective:
      'Compare discovered symptom clusters to published runbook topics and publish CoverageGapDetectedEvent for unmapped high-frequency patterns.',
    sequence: [
      {
        step: 1,
        title: 'Gap Evaluation Handler',
        details: [
          'Handle SymptomClusterDiscoveredEvent by querying published runbook tags.',
          'If coverage is below 60%, record gap and emit CoverageGapDetectedEvent.'
        ]
      }
    ],
    verification: [
      { description: 'Run gap evaluation tests', command: 'dotnet test tests/Modules/GapAnalyzer.Tests/' }
    ],
    dependencies: ['RF-GAPS-001']
  },
  {
    id: 'RF-GAPS-003',
    title: 'Operational Gap Dashboard & Prioritization View',
    module: 'GapAnalyzer',
    schema: 'gaps',
    layer: 'Frontend',
    status: 'PLANNED',
    contextSpecs: [
      'docs/standards/TYPESCRIPT-REACT-STANDARDS.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Build the operational gap dashboard presenting subsystem coverage percentages, unmapped incidents, and one-click runbook draft generation prompts.',
    sequence: [
      {
        step: 1,
        title: 'Coverage Matrix Table',
        details: [
          'Render subsystem coverage cards with severity bars and frequency metrics.'
        ]
      }
    ],
    verification: [
      { description: 'Run frontend test suite', command: 'npm --prefix src/Frontend run test' }
    ],
    dependencies: ['RF-GAPS-002']
  },
  {
    id: 'RF-TRAINING-001',
    title: 'training PostgreSQL Schema Migrations & DbContext',
    module: 'AICriticAndTraining',
    schema: 'training',
    layer: 'AICriticAndTraining.Infrastructure',
    status: 'PLANNED',
    contextSpecs: [
      'docs/database/DB-05-SCHEMA-TRAINING.md',
      'docs/standards/POSTGRES-SQL-STANDARDS.md'
    ],
    objective:
      'Configure TrainingDbContext mapping practice_scenarios and evaluation_logs in schema training.',
    sequence: [
      {
        step: 1,
        title: 'Training Schema Entities',
        details: [
          'Create PracticeScenario and EvaluationLog entities with JSON rubric results.'
        ]
      }
    ],
    verification: [
      { description: 'Build AICritic infrastructure', command: 'dotnet build src/Modules/AICriticAndTraining/Infrastructure' }
    ],
    dependencies: ['RF-CORE-001']
  },
  {
    id: 'RF-TRAINING-002',
    title: 'AI Critic Review Engine (LLM Rubric Evaluation)',
    module: 'AICriticAndTraining',
    schema: 'training',
    layer: 'AICriticAndTraining.Application',
    status: 'PLANNED',
    contextSpecs: [
      'docs/standards/AI-INTEGRATION-STANDARDS.md',
      'docs/01-MODULE-REGISTRY.md'
    ],
    objective:
      'Evaluate runbooks against completeness, clarity, command safety, and idempotency rubrics using schema-enforced JSON structured LLM outputs.',
    sequence: [
      {
        step: 1,
        title: 'Rubric Evaluator Handler',
        details: [
          'Submit runbook AST to LLM provider with strict JSON schema.',
          'Score criteria from 1 to 5 and return detailed improvement suggestions.'
        ]
      }
    ],
    verification: [
      { description: 'Run AI Critic unit tests', command: 'dotnet test tests/Modules/AICriticAndTraining.Tests/' }
    ],
    dependencies: ['RF-TRAINING-001']
  },
  {
    id: 'RF-TRAINING-003',
    title: 'Operator Drill Simulation Engine & Step Scorer',
    module: 'AICriticAndTraining',
    schema: 'training',
    layer: 'AICriticAndTraining.Application',
    status: 'PLANNED',
    contextSpecs: [
      'docs/01-MODULE-REGISTRY.md',
      'docs/02-DATA-FLOW-AND-EVENTS.md'
    ],
    objective:
      'Simulate operator interactive incident drills, evaluating decision speed, correct step execution, and escalation protocol adherence.',
    sequence: [
      {
        step: 1,
        title: 'Drill Session Runner',
        details: [
          'Manage drill state machine and calculate operator readiness score.'
        ]
      }
    ],
    verification: [
      { description: 'Run drill runner unit tests', command: 'dotnet test tests/Modules/AICriticAndTraining.Tests/ --filter "Category=Drills"' }
    ],
    dependencies: ['RF-TRAINING-001']
  }
];

export const MILESTONES_DATA = [
  {
    id: 'M1',
    name: 'Core Architecture & BuildingBlocks',
    targetDate: '2026-11-01',
    status: 'COMPLETED' as const,
    scope: 'Solution scaffolding, Result<T> pattern, MediatR pipeline behaviors, WebApi host, Vite React 19 frontend.',
    tasks: ['RF-CORE-001', 'RF-CORE-002', 'RF-CORE-003', 'RF-CORE-005']
  },
  {
    id: 'M2',
    name: 'F7Hub Read-Only Sync & Ticket Ingestion',
    targetDate: '2026-11-04',
    status: 'UPCOMING' as const,
    scope: 'F7Hub SQLite read-only adapter, sync worker, SHA256 deduplication, ingestion schema migrations.',
    tasks: ['RF-INGEST-001', 'RF-INGEST-002', 'RF-INGEST-003']
  },
  {
    id: 'M3',
    name: 'RunbookEngine AST Parser & Linter',
    targetDate: '2026-11-07',
    status: 'IN_PROGRESS' as const,
    scope: 'runbooks schema migrations, Markdig AST pipeline, RF001–RF099 diagnostic linter, React Flow graph.',
    tasks: ['RF-RUNBOOK-001', 'RF-RUNBOOK-002', 'RF-RUNBOOK-003', 'RF-RUNBOOK-004', 'RF-RUNBOOK-005', 'RF-RUNBOOK-006']
  },
  {
    id: 'M4',
    name: 'Pattern Analytics & Vector Clustering',
    targetDate: '2026-11-11',
    status: 'UPCOMING' as const,
    scope: 'analytics schema, pgvector HNSW index with tsvector fallback, symptom clustering, MTTR calculation.',
    tasks: ['RF-ANALYTICS-001', 'RF-ANALYTICS-002', 'RF-ANALYTICS-003']
  },
  {
    id: 'M5',
    name: 'Gap Analyzer & AI Critic Training',
    targetDate: '2026-11-15',
    status: 'UPCOMING' as const,
    scope: 'gaps and training schemas, automated coverage evaluation, LLM rubric scoring, interactive drills.',
    tasks: ['RF-GAPS-001', 'RF-GAPS-002', 'RF-GAPS-003', 'RF-TRAINING-001', 'RF-TRAINING-002', 'RF-TRAINING-003']
  }
];

