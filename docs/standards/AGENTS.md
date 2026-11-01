# Agent Instructions: Engineering Standards & Style

> **Context Dependencies:**  
> - **Source of Truth:** Engineering Standards Routing (`standards/AGENTS.md`)
> - **Global Agent Instructions:** [../AGENTS.md](../AGENTS.md)  
> - **Conventions & Guardrails:** [../04-CONVENTIONS-AND-GUARDRAILS.md](../04-CONVENTIONS-AND-GUARDRAILS.md)  
> - **Scope:** Code conventions, language idioms, test practices, and linting rules.

## 1. Standards Routing Matrix

| Technology / Engineering Concern | Target Specification File |
| :--- | :--- |
| C# (.NET 9), Minimal APIs, EF Core, MediatR handlers | [`CSHARP-STANDARDS.md`](./CSHARP-STANDARDS.md) |
| React 19, TypeScript strict mode, TanStack Query, Tailwind | [`TYPESCRIPT-REACT-STANDARDS.md`](./TYPESCRIPT-REACT-STANDARDS.md) |
| Markdig AST parsing, node visitors, rules RF001–RF099 | [`MARKDOWN-PARSER-STANDARDS.md`](./MARKDOWN-PARSER-STANDARDS.md) |
| PostgreSQL schema conventions, indexes, JSONB GIN | [`POSTGRES-SQL-STANDARDS.md`](./POSTGRES-SQL-STANDARDS.md) |
| xUnit, Testcontainers, FluentAssertions, Vitest component testing | [`TESTING-STANDARDS.md`](./TESTING-STANDARDS.md) |
| LLM abstraction contracts, structured output schemas, budgets | [`AI-INTEGRATION-STANDARDS.md`](./AI-INTEGRATION-STANDARDS.md) |

---

## 2. Invariants & Rules
- **Zero-Tolerance Nullability:** C# `<Nullable>enable</Nullable>` is mandatory. No unconstrained generic nulls or suppressed compiler warnings.
- **Strict Typing:** No `any` types in TypeScript. Every backend DTO must have an identical frontend type definition.
- **Pure CSS Utility:** Use Tailwind utility classes exclusively; avoid inline styles and custom CSS stylesheets.

---

## 3. Post-Implementation Standards Sync
When implementing new features:
1. **Rule Additions:** If a new static Markdown lint rule is authored, register its error code and behavior inside [`MARKDOWN-PARSER-STANDARDS.md`](./MARKDOWN-PARSER-STANDARDS.md#3-authoritative-diagnostic-rule-catalog-rf001rf099).
2. **Pattern Evolution:** If a shared MediatR pipeline behavior or frontend API hook pattern is established, document the pattern in [`CSHARP-STANDARDS.md`](./CSHARP-STANDARDS.md) or [`TYPESCRIPT-REACT-STANDARDS.md`](./TYPESCRIPT-REACT-STANDARDS.md).
