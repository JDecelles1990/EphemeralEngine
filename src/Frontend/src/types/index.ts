// Canonical TypeScript interfaces mirroring backend DTOs
export interface RunbookSummary {
  readonly id: string;
  readonly title: string;
  readonly slug: string;
  readonly versionNumber: number;
  readonly status: 'Draft' | 'UnderReview' | 'Published' | 'Archived';
  readonly tags: readonly string[];
  readonly updatedAt: string;
}

export interface SyncJobSummary {
  readonly jobId: string;
  readonly sourceSystem: string;
  readonly status: 'Pending' | 'Running' | 'Completed' | 'Failed';
  readonly processedCount: number;
  readonly startedAtUtc: string;
}

