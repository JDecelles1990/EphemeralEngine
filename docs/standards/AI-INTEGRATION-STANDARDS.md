# RunbookForge — AI Integration & LLM Standards
**Document ID:** `docs/standards/AI-INTEGRATION-STANDARDS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** AI Integration & LLM Standards (`standards/AI-INTEGRATION-STANDARDS.md`)
> - **Module Registry & Contracts:** [../01-MODULE-REGISTRY.md](../01-MODULE-REGISTRY.md)  
> - **Training Schema:** [../database/DB-05-SCHEMA-TRAINING.md](../database/DB-05-SCHEMA-TRAINING.md)  
> - **Engineering Standards Routing:** [./AGENTS.md](./AGENTS.md)

---

## 1. Provider Abstraction Architecture

Direct coupling to specific LLM vendor SDKs is prohibited within application handlers. All AI capabilities are abstracted behind clean interfaces in `AICriticAndTraining`:

```csharp
namespace RunbookForge.Modules.AICriticAndTraining.Application.Contracts;

public interface ICriticEngine
{
    Task<Result<AICriticReviewResultDto>> ReviewRunbookAsync(
        string runbookMarkdownAstJson,
        CancellationToken cancellationToken);
}

public interface IEmbeddingGenerator
{
    Task<Result<ReadOnlyMemory<float>>> GenerateEmbeddingAsync(
        string textPayload,
        CancellationToken cancellationToken);
}

public interface IScenarioGenerator
{
    Task<Result<DrillScenarioDefinitionDto>> GeneratePracticeScenarioAsync(
        Guid runbookId,
        string runbookContent,
        CancellationToken cancellationToken);
}
```

---

## 2. Structured JSON Schema Enforcement

All LLM completions intended for programmatic processing must enforce strict JSON schemas matching application contracts (for C# DTO definitions, see [AICriticReviewResultDto in Module Registry](../01-MODULE-REGISTRY.md#62-domain-notifications-dtos)). Unstructured or conversational completions are rejected:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "AICriticReviewPayload",
  "type": "object",
  "properties": {
    "completenessScore": { "type": "number", "minimum": 0.0, "maximum": 1.0 },
    "clarityScore": { "type": "number", "minimum": 0.0, "maximum": 1.0 },
    "safetyScore": { "type": "number", "minimum": 0.0, "maximum": 1.0 },
    "commendations": {
      "type": "array",
      "items": { "type": "string" }
    },
    "deficiencies": {
      "type": "array",
      "items": { "type": "string" }
    },
    "suggestedRemediations": {
      "type": "array",
      "items": { "type": "string" }
    }
  },
  "required": ["completenessScore", "clarityScore", "safetyScore", "commendations", "deficiencies", "suggestedRemediations"],
  "additionalProperties": false
}
```

---

## 3. Token Budgeting & Context Pruning

1. **Maximum Context Window Allocation:**
   - Critic Prompts: Capped at **12,000 tokens**. Markdown ASTs exceeding this budget must be pruned to eliminate raw prose paragraphs, retaining step commands and assertions.
   - Embedding Inputs: Capped at **8,000 tokens** per ticket transcript.
2. **Deterministic Temperature Tuning:**
   - LLM Critic & Evaluation: `Temperature = 0.1` (ensures reproducible, objective scoring).
   - Scenario Generation: `Temperature = 0.4` (allows controlled creative variations of synthetic faults).
3. **Resilience & Circuit Breaking:**
   - LLM HTTP requests must be governed by Polly circuit breakers: 3 consecutive timeouts trip the breaker for 60 seconds, returning `Result.Failure(Error.Failure("AI.ProviderUnavailable", "..."))`.

