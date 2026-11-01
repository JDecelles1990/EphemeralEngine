# Agent Instructions: Global Architecture & Process

> **Scope:** High-level orientation for AI agents across the RunbookForge workspace.

## 1. Topic-to-Document Routing Matrix

| Subject / Task Intent | Target Reference File |
| :--- | :--- |
| System topology, layers, directory structure | [`00-ARCHITECTURE.md`](./00-ARCHITECTURE.md) |
| Master directory of all docs & dependency map | [`00-MASTER-INDEX.md`](./00-MASTER-INDEX.md) |
| Module interfaces, MediatR commands/queries, public DTOs | [`01-MODULE-REGISTRY.md`](./01-MODULE-REGISTRY.md) |
| Inter-module sequence flows & domain event schemas | [`02-DATA-FLOW-AND-EVENTS.md`](./02-DATA-FLOW-AND-EVENTS.md) |
| Step-by-step feature execution pipeline (Pre-flight to Done) | [`03-AI-FEATURE-SOP.md`](./03-AI-FEATURE-SOP.md) |
| Global invariants, `Result<T>` pattern, error codes | [`04-CONVENTIONS-AND-GUARDRAILS.md`](./04-CONVENTIONS-AND-GUARDRAILS.md) |
| Feature roadmaps, active status, completion logs | [`05-FEATURE-STATE-INDEX.md`](./05-FEATURE-STATE-INDEX.md) |
| Language & framework coding conventions | [`standards/AGENTS.md`](./standards/AGENTS.md) |
| Database schemas, migrations & F7Hub adapter | [`database/AGENTS.md`](./database/AGENTS.md) |

---

## 2. Mandatory Pre-Flight Protocol
Before generating code or executing tasks:
1. Consult [`05-FEATURE-STATE-INDEX.md`](./05-FEATURE-STATE-INDEX.md) to confirm the assigned feature is flagged `IN_PROGRESS`.
2. Review [`03-AI-FEATURE-SOP.md`](./03-AI-FEATURE-SOP.md) for the 6-phase implementation lifecycle.
3. Check [`04-CONVENTIONS-AND-GUARDRAILS.md`](./04-CONVENTIONS-AND-GUARDRAILS.md) for non-negotiable architectural rules.

---

## 3. Post-Implementation Documentation Protocol
Immediately after completing code generation and test execution, you must update the following files before marking a task complete:

1. **[`05-FEATURE-STATE-INDEX.md`](./05-FEATURE-STATE-INDEX.md):**
   - Mark the feature status as `DONE`.
   - Record the completed endpoints, handlers, and test coverage metrics.
2. **[`01-MODULE-REGISTRY.md`](./01-MODULE-REGISTRY.md):**
   - Document any new MediatR Commands, Queries, or public DTOs exposed by the module.
3. **[`02-DATA-FLOW-AND-EVENTS.md`](./02-DATA-FLOW-AND-EVENTS.md):**
   - If new domain events or cross-module notifications were introduced, append their event contract and payload schema.
