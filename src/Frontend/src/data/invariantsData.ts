import { ArchitecturalInvariant } from '../types/controlPlane';

export const INVARIANTS_DATA: ArchitecturalInvariant[] = [
  {
    id: 'INV-001',
    title: 'F7Hub Absolute Read-Only Security Barrier',
    severity: 'CRITICAL',
    targetSchemaOrModule: 'TicketIngestion.Infrastructure',
    ruleStatement:
      'All access to F7Hub SQLite must use Mode=ReadOnly;Cache=Shared;. Under no circumstances generate code that inserts, updates, deletes, or migrates F7Hub data. F7HubReadOnlyDbContext must configure global NoTracking and throw InvalidOperationException on SaveChanges or SaveChangesAsync.',
    codeSnippet: `// Invariant Security Guardrail:
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlite("Data Source=f7hub.db;Mode=ReadOnly;Cache=Shared;")
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}

public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    throw new InvalidOperationException("F7Hub is an external read-only database. Writes are forbidden.");
}`,
    verificationMethod:
      'Static analysis scans for SaveChanges in F7Hub adapters + runtime unit test asserting InvalidOperationException upon Add/Update/Remove.',
    status: 'PASS',
    sourceDoc: 'docs/database/DB-06-EXTERNAL-F7HUB-READONLY.md'
  },
  {
    id: 'INV-002',
    title: 'PostgreSQL Schema & Bounded Context Isolation',
    severity: 'CRITICAL',
    targetSchemaOrModule: 'All 5 Modules (runbooks, ingestion, analytics, gaps, training)',
    ruleStatement:
      'Each module strictly owns its designated PostgreSQL schema. Modules NEVER cross-reference another module\'s DbContext directly, nor declare cross-schema foreign keys. All inter-module communication is asynchronous via MediatR events/commands and public contracts.',
    codeSnippet: `// Schema Separation Enforcement:
modelBuilder.HasDefaultSchema(SchemaConstants.Runbooks); // "runbooks"
// Zero cross-schema FK:
// INCORRECT: builder.HasOne<Ticket>().WithMany();
// CORRECT:   builder.Property(x => x.TicketId).IsRequired(); // Unconstrained ID reference`,
    verificationMethod:
      'Zero project references between module Infrastructure assemblies + EF Core migration DDL model validation.',
    status: 'PASS',
    sourceDoc: 'docs/00-ARCHITECTURE.md'
  },
  {
    id: 'INV-003',
    title: 'Deterministic Result Pattern & RFC 7807 Mappings',
    severity: 'HIGH',
    targetSchemaOrModule: 'BuildingBlocks.Common & Web',
    ruleStatement:
      'Business domain errors must never throw exceptions. Handlers must return Result or Result<T>. HTTP endpoints must convert failures to RFC 7807 ProblemDetails with corresponding HTTP status codes (400, 401, 403, 404, 409, 500).',
    codeSnippet: `// Result Pattern Return Contract:
public async Task<Result<RunbookDto>> Handle(CreateRunbookCommand cmd, CancellationToken ct)
{
    if (await _repo.ExistsAsync(cmd.Slug, ct))
        return Result<RunbookDto>.Failure(Error.Conflict("Runbook.SlugExists", "Slug is taken."));

    return Result<RunbookDto>.Success(dto);
}`,
    verificationMethod:
      'xUnit test suite (BuildingBlocks.Common.Tests) covering Result, Result<T>, ValidationBehavior, and ResultExtensions.',
    status: 'PASS',
    sourceDoc: 'docs/04-CONVENTIONS-AND-GUARDRAILS.md'
  },
  {
    id: 'INV-004',
    title: 'Zero-Warning Strict Compiler & Type Parity',
    severity: 'HIGH',
    targetSchemaOrModule: 'Directory.Build.props & Frontend tsconfig',
    ruleStatement:
      '.NET 9 solution enforces <Nullable>enable</Nullable> and <TreatWarningsAsErrors>true</TreatWarningsAsErrors>. Frontend TypeScript enforces strict mode (noImplicitAny, noUnusedLocals) with 1:1 type parity to C# DTO contracts with zero "any".',
    codeSnippet: `<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <LangVersion>13.0</LangVersion>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>`,
    verificationMethod:
      'dotnet build RunbookForge.sln (0 warnings, 0 errors) + npm run build (0 type errors).',
    status: 'PASS',
    sourceDoc: 'docs/standards/CSHARP-STANDARDS.md'
  },
  {
    id: 'INV-005',
    title: 'Markdig AST Parsing & RF001–RF099 Diagnostic Pipeline',
    severity: 'HIGH',
    targetSchemaOrModule: 'RunbookEngine (runbooks)',
    ruleStatement:
      'All runbook content must be parsed through the Markdig AST pipeline supporting custom container blocks (:::step, :::assert). Structural flaws must emit deterministic diagnostic codes (RF001 for missing frontmatter, RF010 for flow gaps, RF030 for unbound timeouts).',
    codeSnippet: `// Markdown AST Custom Container Rule:
var pipeline = new MarkdownPipelineBuilder()
    .UseYamlFrontMatter()
    .UseCustomContainers()
    .Use(new RunbookLinterExtension())
    .Build();

var document = Markdown.Parse(markdownContent, pipeline);`,
    verificationMethod:
      'Markdig AST parser unit tests and diagnostic code verification assertions.',
    status: 'PASS',
    sourceDoc: 'docs/standards/MARKDOWN-PARSER-STANDARDS.md'
  }
];

