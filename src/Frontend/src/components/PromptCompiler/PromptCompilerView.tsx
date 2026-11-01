import React, { useState, useMemo } from 'react';
import {
  Sparkles,
  Sliders,
  Copy,
  Check,
  Terminal,
  FileCode,
  Shield,
  Layers,
  ArrowRight,
  ExternalLink,
  ChevronRight
} from 'lucide-react';
import { TaskDefinition, ModuleCategory } from '../../types/controlPlane';
import { StatusBadge } from '../common/StatusBadge';
import { CodeBlock } from '../common/CodeBlock';

interface PromptCompilerViewProps {
  tasks: TaskDefinition[];
  selectedTaskId: string;
  onSelectTask: (taskId: string) => void;
  onTriggerToast: (message: string) => void;
}

export const PromptCompilerView: React.FC<PromptCompilerViewProps> = ({
  tasks,
  selectedTaskId,
  onSelectTask,
  onTriggerToast
}) => {
  const [selectedModule, setSelectedModule] = useState<ModuleCategory | 'ALL'>('ALL');
  const [testFramework, setTestFramework] = useState<'xUnit' | 'Vitest'>('xUnit');
  const [mockLibrary, setMockLibrary] = useState<'NSubstitute' | 'Moq'>('NSubstitute');
  const [enforceStrictWarnings, setEnforceStrictWarnings] = useState<boolean>(true);
  const [includeProblemDetails, setIncludeProblemDetails] = useState<boolean>(true);
  const [concurrencyToken, setConcurrencyToken] = useState<'xmin' | 'RowVersion'>('xmin');
  const [copiedPrompt, setCopiedPrompt] = useState(false);

  const filteredTasks = useMemo(() => {
    return tasks.filter((t) => (selectedModule === 'ALL' ? true : t.module === selectedModule));
  }, [tasks, selectedModule]);

  const currentTask = useMemo(() => {
    return tasks.find((t) => t.id === selectedTaskId) || tasks[0];
  }, [tasks, selectedTaskId]);

  // Generate the deterministic /run prompt string
  const compiledPrompt = useMemo(() => {
    if (!currentTask) return '';

    const lines: string[] = [];
    lines.push('/run');
    lines.push('');
    lines.push('### Task Context');
    lines.push(`- **Task ID:** \`${currentTask.id}\``);
    lines.push(`- **Scope:** ${currentTask.title}`);
    lines.push(`- **Target Schema / Module:** ${currentTask.schema !== 'none' ? `Schema: \`${currentTask.schema}\`` : 'Shared'} (\`${currentTask.layer}\`)`);
    lines.push('- **Applicable Specs:**');
    currentTask.contextSpecs.forEach((spec) => {
      lines.push(`  - \`${spec}\``);
    });
    lines.push('');
    lines.push('---');
    lines.push('');
    lines.push('### Objective');
    lines.push(currentTask.objective);
    lines.push('');
    lines.push('---');
    lines.push('');
    lines.push('### Step-by-Step Implementation Sequence');
    lines.push('');

    currentTask.sequence.forEach((seq) => {
      lines.push(`#### ${seq.step}. ${seq.title}`);
      seq.details.forEach((detail) => {
        lines.push(`- ${detail}`);
      });
      lines.push('');
    });

    lines.push('---');
    lines.push('');
    lines.push('### Verification & Gate');
    currentTask.verification.forEach((verif, idx) => {
      lines.push(`${idx + 1}. Execute \`${verif.command}\` — ${verif.description}.`);
    });
    lines.push(`${currentTask.verification.length + 1}. Update \`docs/05-FEATURE-STATE-INDEX.md\`:`);
    lines.push(`   - Mark \`${currentTask.id}\` as \`COMPLETED\`.`);
    lines.push('   - Mark the next sequential task as `IN_PROGRESS`.');

    return lines.join('\n');
  }, [currentTask]);

  // Rough token estimation: 1 token ~ 4 characters
  const estimatedTokens = useMemo(() => {
    return Math.round(compiledPrompt.length / 4);
  }, [compiledPrompt]);

  const handleCopyPrompt = async () => {
    try {
      await navigator.clipboard.writeText(compiledPrompt);
      setCopiedPrompt(true);
      onTriggerToast(`Prompt for ${currentTask.id} copied to clipboard! Ready for Antigravity IDE.`);
      setTimeout(() => setCopiedPrompt(false), 2000);
    } catch {
      // Fallback
      const textArea = document.createElement('textarea');
      textArea.value = compiledPrompt;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand('copy');
      document.body.removeChild(textArea);
      setCopiedPrompt(true);
      onTriggerToast(`Prompt for ${currentTask.id} copied to clipboard!`);
      setTimeout(() => setCopiedPrompt(false), 2000);
    }
  };

  const moduleCategories: (ModuleCategory | 'ALL')[] = [
    'ALL',
    'Core',
    'RunbookEngine',
    'TicketIngestion',
    'PatternAnalytics',
    'GapAnalyzer',
    'AICriticAndTraining'
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Top Banner / Explanation */}
      <div className="mb-8 rounded-xl border border-slate-800 bg-gradient-to-r from-slate-900 via-indigo-950/20 to-slate-900 p-6 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 mb-2">
            <span className="p-1 rounded bg-indigo-500/10 text-indigo-400 border border-indigo-500/30">
              <Sparkles className="w-4 h-4" />
            </span>
            <h2 className="text-lg font-semibold text-slate-100">Antigravity Execution Prompt Compiler</h2>
            <span className="px-2 py-0.5 rounded text-[11px] font-mono bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">
              Zero-Hallucination SOP
            </span>
          </div>
          <p className="text-xs text-slate-400 max-w-2xl leading-relaxed">
            Synthesizes strictly bounded <code className="text-indigo-300 font-mono">/run</code> prompts for coding agents in Antigravity IDE and Google AI Studio. Enforces the 3–4 specification slice limit, non-negotiable architectural invariants, and verification gates.
          </p>
        </div>

        <button
          onClick={handleCopyPrompt}
          className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 transition-all shrink-0 active:scale-95"
        >
          {copiedPrompt ? <Check className="w-4 h-4 text-emerald-300" /> : <Copy className="w-4 h-4" />}
          <span>{copiedPrompt ? 'Copied to Clipboard!' : 'Copy /run Execution Prompt'}</span>
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        {/* Left Column: Task Catalog & Slicing Controls (5 cols) */}
        <div className="lg:col-span-5 space-y-6">
          {/* Module Filter Pills */}
          <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
            <label className="text-xs font-mono uppercase tracking-wider text-slate-400 block mb-3">
              1. Filter Backlog By Module
            </label>
            <div className="flex flex-wrap gap-1.5">
              {moduleCategories.map((mod) => (
                <button
                  key={mod}
                  onClick={() => setSelectedModule(mod)}
                  className={`px-2.5 py-1 rounded-md text-xs font-medium transition-colors ${
                    selectedModule === mod
                      ? 'bg-indigo-600 text-white'
                      : 'bg-slate-800/80 text-slate-400 hover:text-slate-200 hover:bg-slate-700/80'
                  }`}
                >
                  {mod}
                </button>
              ))}
            </div>
          </div>

          {/* Task Selector List */}
          <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
            <div className="flex items-center justify-between mb-3">
              <label className="text-xs font-mono uppercase tracking-wider text-slate-400">
                2. Select Implementation Slice ({filteredTasks.length})
              </label>
              <span className="text-[11px] text-slate-400 font-mono">
                Active: <span className="text-indigo-400 font-semibold">{currentTask.id}</span>
              </span>
            </div>

            <div className="space-y-2 max-h-[380px] overflow-y-auto pr-1">
              {filteredTasks.map((task) => {
                const isSelected = task.id === currentTask.id;
                return (
                  <div
                    key={task.id}
                    onClick={() => onSelectTask(task.id)}
                    className={`p-3 rounded-lg border text-left cursor-pointer transition-all ${
                      isSelected
                        ? 'bg-indigo-950/40 border-indigo-500/60 shadow-md shadow-indigo-950/50'
                        : 'bg-slate-950/40 border-slate-800/80 hover:border-slate-700 hover:bg-slate-800/30'
                    }`}
                  >
                    <div className="flex items-center justify-between gap-2 mb-1.5">
                      <div className="flex items-center gap-2">
                        <span className="font-mono text-xs font-bold text-slate-200">{task.id}</span>
                        <span className="text-[10px] font-mono px-1.5 py-0.2 rounded bg-slate-800 text-slate-400">
                          {task.module}
                        </span>
                      </div>
                      <StatusBadge status={task.status} size="sm" />
                    </div>
                    <p className="text-xs text-slate-300 font-medium line-clamp-1">{task.title}</p>
                    <div className="mt-2 flex items-center justify-between text-[11px] text-slate-400 font-mono">
                      <span>{task.contextSpecs.length} specs sliced</span>
                      <span className="flex items-center gap-1 text-indigo-400">
                        {isSelected ? 'Selected' : 'Select'}
                        <ChevronRight className="w-3 h-3" />
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Context Specification Slices */}
          <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
            <div className="flex items-center justify-between mb-3">
              <label className="text-xs font-mono uppercase tracking-wider text-slate-400 flex items-center gap-1.5">
                <FileCode className="w-3.5 h-3.5 text-indigo-400" />
                Attached Context Specs (Strict Limit ≤ 4)
              </label>
              <span className="text-[11px] font-mono text-emerald-400">
                {currentTask.contextSpecs.length} / 4 specs
              </span>
            </div>

            <div className="space-y-1.5">
              {currentTask.contextSpecs.map((spec) => (
                <div
                  key={spec}
                  className="flex items-center justify-between px-3 py-2 rounded-lg bg-slate-950/60 border border-slate-800 text-xs font-mono text-slate-300"
                >
                  <span className="truncate">{spec}</span>
                  <ExternalLink className="w-3 h-3 text-slate-400 shrink-0 ml-2" />
                </div>
              ))}
            </div>
          </div>

          {/* Execution Options & Flags */}
          <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
            <label className="text-xs font-mono uppercase tracking-wider text-slate-400 flex items-center gap-1.5 mb-3">
              <Sliders className="w-3.5 h-3.5 text-sky-400" />
              Compiler Execution Flags
            </label>

            <div className="space-y-3 text-xs">
              <div className="flex items-center justify-between">
                <span className="text-slate-300 font-medium">Test Framework</span>
                <div className="flex gap-1 bg-slate-950 p-1 rounded-md border border-slate-800">
                  <button
                    onClick={() => setTestFramework('xUnit')}
                    className={`px-2 py-0.5 rounded text-[11px] font-mono ${
                      testFramework === 'xUnit' ? 'bg-indigo-600 text-white' : 'text-slate-400'
                    }`}
                  >
                    xUnit 2.9
                  </button>
                  <button
                    onClick={() => setTestFramework('Vitest')}
                    className={`px-2 py-0.5 rounded text-[11px] font-mono ${
                      testFramework === 'Vitest' ? 'bg-indigo-600 text-white' : 'text-slate-400'
                    }`}
                  >
                    Vitest
                  </button>
                </div>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-slate-300 font-medium">Mocking Engine</span>
                <div className="flex gap-1 bg-slate-950 p-1 rounded-md border border-slate-800">
                  <button
                    onClick={() => setMockLibrary('NSubstitute')}
                    className={`px-2 py-0.5 rounded text-[11px] font-mono ${
                      mockLibrary === 'NSubstitute' ? 'bg-indigo-600 text-white' : 'text-slate-400'
                    }`}
                  >
                    NSubstitute
                  </button>
                  <button
                    onClick={() => setMockLibrary('Moq')}
                    className={`px-2 py-0.5 rounded text-[11px] font-mono ${
                      mockLibrary === 'Moq' ? 'bg-indigo-600 text-white' : 'text-slate-400'
                    }`}
                  >
                    Moq
                  </button>
                </div>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-slate-300 font-medium">Concurrency Token</span>
                <div className="flex gap-1 bg-slate-950 p-1 rounded-md border border-slate-800">
                  <button
                    onClick={() => setConcurrencyToken('xmin')}
                    className={`px-2 py-0.5 rounded text-[11px] font-mono ${
                      concurrencyToken === 'xmin' ? 'bg-indigo-600 text-white' : 'text-slate-400'
                    }`}
                  >
                    PostgreSQL xmin
                  </button>
                  <button
                    onClick={() => setConcurrencyToken('RowVersion')}
                    className={`px-2 py-0.5 rounded text-[11px] font-mono ${
                      concurrencyToken === 'RowVersion' ? 'bg-indigo-600 text-white' : 'text-slate-400'
                    }`}
                  >
                    RowVersion
                  </button>
                </div>
              </div>

              <div className="flex items-center justify-between pt-1 border-t border-slate-800/60">
                <span className="text-slate-300 font-medium">TreatWarningsAsErrors</span>
                <input
                  type="checkbox"
                  checked={enforceStrictWarnings}
                  onChange={(e) => setEnforceStrictWarnings(e.target.checked)}
                  className="rounded border-slate-700 text-indigo-600 focus:ring-0 bg-slate-950"
                />
              </div>

              <div className="flex items-center justify-between">
                <span className="text-slate-300 font-medium">RFC 7807 ProblemDetails</span>
                <input
                  type="checkbox"
                  checked={includeProblemDetails}
                  onChange={(e) => setIncludeProblemDetails(e.target.checked)}
                  className="rounded border-slate-700 text-indigo-600 focus:ring-0 bg-slate-950"
                />
              </div>
            </div>
          </div>
        </div>

        {/* Right Column: Live Compiled /run Prompt Preview (7 cols) */}
        <div className="lg:col-span-7 space-y-6">
          {/* Header & Token Counter */}
          <div className="flex items-center justify-between bg-slate-900/60 border border-slate-800 rounded-xl px-5 py-3">
            <div className="flex items-center gap-2">
              <Terminal className="w-4 h-4 text-indigo-400" />
              <span className="text-xs font-mono font-semibold text-slate-200">
                /run Prompt Live Preview ({currentTask.id})
              </span>
            </div>

            <div className="flex items-center gap-3 text-xs font-mono">
              <span className="text-slate-400">
                Est. Tokens: <span className="text-sky-400 font-semibold">{estimatedTokens}</span>
              </span>
              <span className="text-slate-600">·</span>
              <span className="text-emerald-400 font-medium flex items-center gap-1">
                <Shield className="w-3 h-3 inline" /> Antigravity Verified
              </span>
            </div>
          </div>

          {/* Formatted Code Block with Copy */}
          <CodeBlock
            code={compiledPrompt}
            language="markdown"
            title={`Antigravity IDE Execution Prompt — ${currentTask.id}`}
            showLineNumbers={true}
          />

          {/* CLI Verification Scripts Box */}
          <div className="rounded-xl border border-slate-800 bg-slate-900/40 p-5">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-xs font-mono uppercase tracking-wider text-slate-300 flex items-center gap-1.5">
                <Terminal className="w-3.5 h-3.5 text-emerald-400" />
                CLI Verification Gates for this Slice
              </h3>
              <span className="text-[11px] font-mono text-slate-400">PowerShell 7 / pwsh</span>
            </div>

            <div className="space-y-2">
              {currentTask.verification.map((v, i) => (
                <div
                  key={i}
                  className="p-3 rounded-lg bg-slate-950/80 border border-slate-800/80 flex flex-col gap-1 text-xs"
                >
                  <div className="flex items-center justify-between text-slate-400">
                    <span className="font-sans font-medium text-slate-300">{v.description}</span>
                    <button
                      onClick={async () => {
                        await navigator.clipboard.writeText(v.command);
                        onTriggerToast(`Copied command: ${v.command}`);
                      }}
                      className="text-indigo-400 hover:text-indigo-300 flex items-center gap-1 font-mono text-[10px]"
                    >
                      <Copy className="w-3 h-3" /> Copy
                    </button>
                  </div>
                  <code className="text-emerald-400 font-mono text-[11px] select-all bg-slate-900/80 px-2 py-1 rounded">
                    {v.command}
                  </code>
                </div>
              ))}
            </div>
          </div>

          {/* Upstream / Downstream Dependency Card */}
          <div className="rounded-xl border border-slate-800 bg-slate-900/40 p-5">
            <h3 className="text-xs font-mono uppercase tracking-wider text-slate-300 flex items-center gap-1.5 mb-3">
              <Layers className="w-3.5 h-3.5 text-indigo-400" />
              Task Dependency Graph
            </h3>

            <div className="flex flex-wrap items-center gap-2 text-xs font-mono">
              {currentTask.dependencies.length > 0 ? (
                <>
                  <span className="text-slate-400">Upstream Required:</span>
                  {currentTask.dependencies.map((dep) => (
                    <button
                      key={dep}
                      onClick={() => onSelectTask(dep)}
                      className="px-2 py-0.5 rounded bg-slate-800 text-indigo-300 border border-slate-700 hover:border-indigo-500 transition-colors"
                    >
                      {dep}
                    </button>
                  ))}
                  <ArrowRight className="w-3.5 h-3.5 text-slate-600 mx-1" />
                </>
              ) : (
                <span className="text-slate-400">Zero upstream dependencies (Root Slice).</span>
              )}
              <span className="px-2.5 py-1 rounded bg-indigo-600/20 text-indigo-300 border border-indigo-500/40 font-bold">
                {currentTask.id}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

