# RunbookForge — Markdig AST Pipeline & Linter Rules (RF001–RF099)
**Document ID:** `docs/standards/MARKDOWN-PARSER-STANDARDS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** Markdig AST Extensions & Lint Rules (`standards/MARKDOWN-PARSER-STANDARDS.md`)
> - **Module Registry & DTOs:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)  
> - **Engineering Standards Routing:** [./AGENTS.md](./AGENTS.md)  
> - **Schema Spec:** [../database/DB-01-SCHEMA-RUNBOOKS.md](../database/DB-01-SCHEMA-RUNBOOKS.md)

---

## 1. Markdig AST Extension Pipeline

RunbookForge extends the .NET 9 **Markdig** parser to process Markdown documents into an Abstract Syntax Tree (AST) representing executable operational workflows:

```csharp
public static class RunbookMarkdownPipeline
{
    public static MarkdownPipeline Create() =>
        new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UseCustomContainers() // Step & validation blocks
            .UseGenericAttributes() // Attributes on code fences
            .UseRunbookExtensions() // Custom RF AST visitors
            .Build();
}
```

---

## 2. Structural Grammar & Custom Fences

A valid RunbookForge document follows this structural grammar:

```markdown
---
title: "Database Connection Pool Recovery"
slug: "db-connection-pool-recovery"
version: 1.0.0
schema_version: "rf-v1"
author: "DevOps Core"
tags: ["postgres", "p1-incident", "infrastructure"]
---

# Overview
Remediation procedure when connection pool reaches 100% saturation.

:::step id="step-1" title="Inspect Active Connections"
Check the current connection count against max pool limits.

```bash {lang="bash" timeout="15s" mode="readonly"}
psql -h $PGHOST -U $PGUSER -c "SELECT count(*), state FROM pg_stat_activity GROUP BY state;"
```

:::assert target="stdout" condition="matches" value="^[0-9]+"
Ensure the query returns integer state metrics.
:::
:::

:::step id="step-2" title="Terminate Idle In Transaction Sessions"
Kill rogue client connections lingering in an open transaction state.

```bash {lang="bash" timeout="30s"}
psql -h $PGHOST -U $PGUSER -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE state = 'idle in transaction' AND state_change < now() - INTERVAL '5 minutes';"
```
:::
```

---

## 3. Authoritative Diagnostic Rule Catalog (`RF001`–`RF099`)

Every parsed document is evaluated against the diagnostic catalog. Violations produce a `LintDiagnosticDto` (for contract declaration, see [Module Registry DTO Specifications](../01-MODULE-REGISTRY.md#22-domain-notifications-dtos)).

### 3.1 Frontmatter Diagnostics (`RF001`–`RF019`)
| Rule ID | Severity | Diagnostic Condition | Remediation Suggestion |
| :--- | :--- | :--- | :--- |
| `RF001` | **Error** | Missing YAML frontmatter block at root. | Add `---` frontmatter block at top of document. |
| `RF002` | **Error** | Frontmatter missing required `title` property. | Define `title: "..."` in frontmatter. |
| `RF003` | **Error** | Frontmatter missing required `slug` property. | Define `slug: "..."` matching regex `^[a-z0-9-]+$`. |
| `RF004` | **Error** | `schema_version` is missing or unsupported. | Set `schema_version: "rf-v1"`. |
| `RF005` | **Warning** | Frontmatter missing `tags` collection or empty. | Provide at least one search tag in `tags: [...]`. |
| `RF006` | **Warning** | Frontmatter `version` does not conform to semver. | Format version as `MAJOR.MINOR.PATCH` (e.g., `1.0.0`). |

### 3.2 Step Sequencing & Flow Diagnostics (`RF020`–`RF039`)
| Rule ID | Severity | Diagnostic Condition | Remediation Suggestion |
| :--- | :--- | :--- | :--- |
| `RF020` | **Error** | Document contains no `:::step` containers. | Runbook must contain at least one defined step. |
| `RF021` | **Error** | Duplicate `id` attribute found across `:::step` nodes. | Ensure every `id` attribute is globally unique. |
| `RF022` | **Error** | Step references an undefined target in `next_step`. | Correct the step pointer or remove dangling reference. |
| `RF023` | **Error** | Circular reference loop detected in step graph. | Resolve circular dependencies between step nodes. |
| `RF024` | **Warning** | Orphaned step node detected (unreachable from root). | Connect step into execution graph or delete step. |

### 3.3 Executable Block & Code Fence Diagnostics (`RF040`–`RF059`)
| Rule ID | Severity | Diagnostic Condition | Remediation Suggestion |
| :--- | :--- | :--- | :--- |
| `RF040` | **Error** | Code fence inside step missing language tag. | Specify language attribute, e.g., `bash`, `powershell`. |
| `RF041` | **Error** | Prohibited destructive command in read-only step. | Remove write commands (`rm -rf`, `DROP TABLE`, `UPDATE`). |
| `RF042` | **Error** | Missing required `timeout` attribute on executable fence. | Specify timeout, e.g., `{timeout="30s"}`. |
| `RF043` | **Warning** | Timeout value exceeds maximum allowed limit (300s). | Reduce timeout or decompose into multiple steps. |

### 3.4 Assertions & Verification Diagnostics (`RF060`–`RF079`)
| Rule ID | Severity | Diagnostic Condition | Remediation Suggestion |
| :--- | :--- | :--- | :--- |
| `RF060` | **Error** | `:::assert` block missing required `condition` attribute. | Specify `condition="equals|contains|matches"`. |
| `RF061` | **Error** | `:::assert` target attribute is invalid. | Target must be one of `stdout`, `stderr`, `exit_code`. |
| `RF062` | **Error** | Regex syntax error in `value` attribute for regex assert. | Fix regular expression syntax. |
| `RF063` | **Warning** | Step contains execution script without verification assert. | Add an assertion block to verify step outcome. |

### 3.5 Semantic & Operational Warnings (`RF080`–`RF099`)
| Rule ID | Severity | Diagnostic Condition | Remediation Suggestion |
| :--- | :--- | :--- | :--- |
| `RF080` | **Warning** | Destructive step lacks corresponding rollback block. | Define a `:::rollback` container for remediation. |
| `RF081` | **Warning** | Ambiguous instruction containing placeholder tokens. | Replace `<YOUR_TOKEN_HERE>` with concrete parameter. |
| `RF082` | **Info** | Deprecated CLI tool detected in command block. | Migrate to modern recommended CLI tool. |


---

## 4. Workflow DAG Compilation (Branching)

`WorkflowDagCompiler` turns a parsed `RunbookAstDocument` into a React Flow DAG (`Start → Step… → Terminal`, plus one `Escalation` sink per `# Escalation` bullet). The following authoring rules are authoritative:

| Authoring construct | Compiled result |
| :--- | :--- |
| Consecutive `:::step` containers | Sequential edges `Step(i) → Step(i+1)`; last step → `Terminal`. |
| `:::question prompt="..." options="Yes,No"` inside a step | `Decision` node `{stepId}__decision_{n}` after the step; one labelled outgoing edge per option. Plain options fall through to the next decision/step. |
| Option written `Label->step-id` (e.g. `options="Fixed,Retry->step-1"`) | Edge labelled `Label` jumping to `step-id`. Unknown target → `Workflow.InvalidBranchTarget`. |
| Multiple questions in one step | Chained decisions (`__decision_1`, `__decision_2`, …). |

**Graph invariants** (violations return `Result.Failure(Error.Validation(...))`, never an exception):

- Acyclic — back-edges (e.g. `Retry->step-1` from a later step) yield `Workflow.CycleDetected`. Detection uses an iterative 3-colour DFS; ranking uses Kahn's algorithm.
- Every step is reachable from `Start` — otherwise `Workflow.UnreachableStep`.
- Step ids are unique (`Workflow.DuplicateStepId`) and at least one step exists (`Workflow.NoSteps`).

**Layout:** `Y = depth × 150` (longest-path rank); `X = 250 + (lane − center) × 280` where lanes are the nodes sharing a rank, in declaration order.

**Serialization:** nodes → `{ id, type, position: { x, y }, data: { label, ... } }` with `type ∈ start|step|decision|escalation|terminal`; edges → `{ id, source, target, label?, animated }`. Contract DTO: [`RunbookWorkflowDto`](../01-MODULE-REGISTRY.md#22-domain-notifications-dtos).