# RunbookForge — TypeScript & React 19 Frontend Standards
**Document ID:** `docs/standards/TYPESCRIPT-REACT-STANDARDS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** TypeScript & React 19 Standards (`standards/TYPESCRIPT-REACT-STANDARDS.md`)
> - **Global Invariants:** [../04-CONVENTIONS-AND-GUARDRAILS.md](../04-CONVENTIONS-AND-GUARDRAILS.md)  
> - **Module Registry & DTOs:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)  
> - **Workflow Schema:** [../database/DB-01-SCHEMA-RUNBOOKS.md](../database/DB-01-SCHEMA-RUNBOOKS.md)

---

## 1. Toolchain & Strict TypeScript Invariants

- **Framework:** React 19
- **Bundler:** Vite 6+
- **TypeScript Configuration:** `strict: true`, `noImplicitAny: true`, `exactOptionalPropertyTypes: true`
- **Zero-`any` Policy:** Explicit interfaces or discriminated unions must be used for all state.
- **DTO Synchronization:** All frontend types in `src/Frontend/src/types/` must mirror backend C# DTO contracts 1:1. For canonical DTO field contracts, see [Module Registry DTO Specifications](../01-MODULE-REGISTRY.md#22-domain-notifications-dtos).

---

## 2. TanStack Query v5 Key Factory Architecture

Direct inline string arrays for query keys (e.g., `["runbooks", id]`) are prohibited. All query keys must be managed through centralized key factories:

```typescript
// src/Frontend/src/api/queryKeyFactory.ts
export const runbookKeys = {
  all: ['runbooks'] as const,
  lists: () => [...runbookKeys.all, 'list'] as const,
  list: (filters: { tag?: string; page: number }) => [...runbookKeys.lists(), filters] as const,
  details: () => [...runbookKeys.all, 'detail'] as const,
  detail: (id: string) => [...runbookKeys.details(), id] as const,
  ast: (id: string, version?: number) => [...runbookKeys.detail(id), 'ast', version] as const,
};
```

### Custom Query & Mutation Hooks
```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { runbookKeys } from './queryKeyFactory';
import { fetchRunbookById, updateRunbookContent } from './runbookClient';

export function useRunbook(id: string) {
  return useQuery({
    queryKey: runbookKeys.detail(id),
    queryFn: () => fetchRunbookById(id),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

export function useUpdateRunbook() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateRunbookContent,
    onSuccess: (data) => {
      // Invalidate detail cache on mutation
      queryClient.invalidateQueries({ queryKey: runbookKeys.detail(data.id) });
      queryClient.invalidateQueries({ queryKey: runbookKeys.lists() });
    },
  });
}
```

---

## 3. React Flow Canvas & Step Graph Standards

RunbookForge visualizes runbook execution trees using **React Flow**.

### 3.1 Custom Node Types
- `StepNode`: Standard interactive step requiring manual operator confirmation.
- `AutomatedQueryNode`: Step executing a read-only telemetry or health check script.
- `ValidationNode`: Automated assertion checkpoint evaluating system state.
- `DecisionNode`: Branching node routing flow based on validation result.

```typescript
// src/Frontend/src/features/runbooks/nodes/StepNode.tsx
import React, { memo } from 'react';
import { Handle, Position, NodeProps } from 'reactflow';

export interface StepNodeData {
  stepId: string;
  title: string;
  stepType: 'Interactive' | 'AutomatedQuery' | 'ValidationAssert';
  commandScript?: string;
  isCompleted?: boolean;
}

export const StepNode = memo(({ data }: NodeProps<StepNodeData>) => {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-900 p-4 shadow-md w-64 text-slate-100">
      <Handle type="target" position={Position.Top} className="w-2 h-2 bg-sky-400" />
      <div className="flex items-center justify-between">
        <span className="text-xs font-semibold text-sky-400">{data.stepType}</span>
        {data.isCompleted && <span className="text-xs text-emerald-400">✓ Done</span>}
      </div>
      <h4 className="mt-1 font-medium text-sm truncate">{data.title}</h4>
      {data.commandScript && (
        <code className="mt-2 block bg-slate-950 p-1.5 rounded font-mono text-xs text-slate-300 truncate">
          {data.commandScript}
        </code>
      )}
      <Handle type="source" position={Position.Bottom} className="w-2 h-2 bg-sky-400" />
    </div>
  );
});
```

---

## 4. Tailwind CSS Conventions

- **Component Layering:** Common styles must be extracted using `@layer components` or modular component wrappers rather than duplicated 20-class string literals.
- **Theme Tokens:** Rely strictly on Tailwind slate/sky/emerald palette tokens for high contrast and accessibility.
- **Responsive Layout:** Mobile-friendly sidebar collapsing with accessible focus rings (`focus:ring-2 focus:ring-sky-500`).
