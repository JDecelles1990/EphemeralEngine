export type TaskStatus = 'COMPLETED' | 'IN_PROGRESS' | 'PLANNED' | 'BLOCKED';

export type ModuleCategory =
  | 'Core'
  | 'RunbookEngine'
  | 'TicketIngestion'
  | 'PatternAnalytics'
  | 'GapAnalyzer'
  | 'AICriticAndTraining';

export type SchemaName =
  | 'runbooks'
  | 'ingestion'
  | 'analytics'
  | 'gaps'
  | 'training'
  | 'external_sqlite'
  | 'none';

export interface TaskStep {
  step: number;
  title: string;
  details: string[];
}

export interface TaskVerification {
  description: string;
  command: string;
}

export interface TaskDefinition {
  id: string;
  title: string;
  module: ModuleCategory;
  schema: SchemaName;
  layer: string;
  status: TaskStatus;
  contextSpecs: string[];
  objective: string;
  sequence: TaskStep[];
  verification: TaskVerification[];
  dependencies: string[];
  notes?: string;
  options?: {
    testFramework?: 'xUnit' | 'Vitest';
    mockLibrary?: 'NSubstitute' | 'Moq';
    includeProblemDetails?: boolean;
    concurrencyToken?: 'xmin' | 'RowVersion';
    vectorSearch?: 'pgvector' | 'tsvector-fallback';
    strictErrors?: boolean;
  };
}

export interface ArchitecturalInvariant {
  id: string;
  title: string;
  severity: 'CRITICAL' | 'HIGH';
  targetSchemaOrModule: string;
  ruleStatement: string;
  codeSnippet: string;
  verificationMethod: string;
  status: 'PASS' | 'VIOLATION' | 'UNCHECKED';
  sourceDoc: string;
}

export interface ModuleTopology {
  id: ModuleCategory;
  displayName: string;
  schema: SchemaName;
  dbContext: string;
  primaryTables: string[];
  inboundEvents: string[];
  outboundEvents: string[];
  publicCommands: string[];
  publicQueries: string[];
  readOnlyBoundary?: boolean;
  description: string;
}

export interface SpecDocument {
  id: string;
  docPath: string;
  title: string;
  category: 'System' | 'Standards' | 'Database';
  ssotDomain: string;
  summary: string;
  keyDirectives: string[];
  contentSample: string;
}

export interface Milestone {
  id: string;
  name: string;
  targetDate: string;
  status: 'COMPLETED' | 'IN_PROGRESS' | 'UPCOMING';
  scope: string;
  tasks: string[];
}

