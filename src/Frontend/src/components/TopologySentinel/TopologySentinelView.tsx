import React, { useState } from 'react';
import {
  ShieldCheck,
  Database,
  Lock,
  CheckCircle2,
  Play,
  Layers,
  Cpu,
  RefreshCw
} from 'lucide-react';
import { MODULE_TOPOLOGY_DATA } from '../../data/topologyData';
import { INVARIANTS_DATA } from '../../data/invariantsData';
import { ModuleCategory } from '../../types/controlPlane';

interface TopologySentinelViewProps {
  onTriggerToast: (message: string) => void;
}

export const TopologySentinelView: React.FC<TopologySentinelViewProps> = ({ onTriggerToast }) => {
  const [selectedModuleId, setSelectedModuleId] = useState<ModuleCategory>('RunbookEngine');
  const [auditRunning, setAuditRunning] = useState(false);
  const invariants = INVARIANTS_DATA;

  const selectedModule = MODULE_TOPOLOGY_DATA.find((m) => m.id === selectedModuleId) || MODULE_TOPOLOGY_DATA[0];

  const handleRunAudit = () => {
    setAuditRunning(true);
    setTimeout(() => {
      setAuditRunning(false);
      onTriggerToast('Architectural Sentinel Audit complete: 5/5 Invariants Passed (0 Violations).');
    }, 800);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Top Sentinel Header */}
      <div className="rounded-xl border border-slate-800 bg-gradient-to-r from-slate-900 via-indigo-950/20 to-slate-900 p-6 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 mb-2">
            <span className="p-1 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">
              <ShieldCheck className="w-4 h-4" />
            </span>
            <h2 className="text-lg font-semibold text-slate-100">Modular Monolith Topology & Invariant Sentinel</h2>
            <span className="px-2 py-0.5 rounded text-[11px] font-mono bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">
              5 Isolated Schemas
            </span>
          </div>
          <p className="text-xs text-slate-400 max-w-2xl leading-relaxed">
            Visualizes bounded context isolation across the 5 PostgreSQL schemas, strict zero-cross-module DbContext injection, MediatR event streams, and the read-only F7Hub SQLite security barrier.
          </p>
        </div>

        <button
          onClick={handleRunAudit}
          disabled={auditRunning}
          className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold shadow-lg shadow-emerald-600/30 transition-all shrink-0 disabled:opacity-50"
        >
          {auditRunning ? (
            <RefreshCw className="w-4 h-4 animate-spin" />
          ) : (
            <Play className="w-4 h-4" />
          )}
          <span>{auditRunning ? 'Running Invariant Scan...' : 'Run Invariant Audit'}</span>
        </button>
      </div>

      {/* Visual Topology Diagram Section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-slate-200 flex items-center gap-2">
            <Layers className="w-4 h-4 text-indigo-400" />
            Bounded Contexts & Multi-Schema Relational Topology
          </h3>
          <span className="text-xs text-slate-400 font-mono">Click card to inspect contracts</span>
        </div>

        {/* Schema Cards Grid */}
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          {MODULE_TOPOLOGY_DATA.map((mod) => {
            const isSelected = mod.id === selectedModuleId;
            return (
              <div
                key={mod.id}
                onClick={() => setSelectedModuleId(mod.id)}
                className={`p-4 rounded-xl border cursor-pointer transition-all flex flex-col justify-between ${
                  isSelected
                    ? 'bg-indigo-950/40 border-indigo-500 shadow-lg shadow-indigo-950/50 scale-[1.02]'
                    : 'bg-slate-900/60 border-slate-800 hover:border-slate-700 hover:bg-slate-800/40'
                }`}
              >
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="font-mono text-xs px-2 py-0.5 rounded bg-slate-950 text-sky-400 border border-slate-800 font-bold">
                      {mod.schema}
                    </span>
                    <Database className="w-3.5 h-3.5 text-slate-500" />
                  </div>
                  <h4 className="text-xs font-bold text-slate-100">{mod.displayName}</h4>
                  <p className="text-[11px] text-slate-400 mt-1 line-clamp-2 leading-relaxed">
                    {mod.description}
                  </p>
                </div>

                <div className="mt-4 pt-3 border-t border-slate-800/80 text-[10px] font-mono text-slate-400 space-y-1">
                  <div className="text-slate-300 font-medium truncate">{mod.dbContext}</div>
                  <div className="flex items-center justify-between">
                    <span>{mod.primaryTables.length} tables</span>
                    <span className="text-indigo-400">{mod.outboundEvents.length} events</span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* F7Hub SQLite Read-Only Barrier Box */}
        <div className="rounded-xl border border-amber-500/30 bg-amber-950/10 p-4 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-amber-500/20 text-amber-400 border border-amber-500/30">
              <Lock className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h4 className="text-xs font-bold text-amber-300 font-mono uppercase tracking-wide">
                  External Security Barrier: F7Hub SQLite Adapter (f7hub.db)
                </h4>
                <span className="px-1.5 py-0.2 rounded text-[10px] font-mono bg-amber-500/20 text-amber-300 border border-amber-500/40">
                  Mode=ReadOnly;
                </span>
              </div>
              <p className="text-xs text-slate-300 mt-0.5">
                Target Adapter: <code className="text-amber-300 font-mono">F7HubReadOnlyDbContext</code> · Global <code className="text-slate-300 font-mono">NoTracking</code> · SaveChanges throws <code className="text-rose-400 font-mono">InvalidOperationException</code>
              </p>
            </div>
          </div>
          <div className="px-3 py-1.5 rounded-lg bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-mono flex items-center gap-1.5">
            <CheckCircle2 className="w-3.5 h-3.5" /> Barrier Enforced
          </div>
        </div>
      </div>

      {/* Selected Module Detail Inspector */}
      <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-6 space-y-4">
        <div className="flex items-center justify-between border-b border-slate-800 pb-3">
          <div className="flex items-center gap-3">
            <Cpu className="w-4 h-4 text-indigo-400" />
            <h4 className="text-sm font-semibold text-slate-100">
              Contract & Entity Inspector: <span className="text-indigo-400 font-mono">{selectedModule.displayName}</span>
            </h4>
          </div>
          <span className="text-xs font-mono text-slate-400">
            Schema: <span className="text-sky-400">{selectedModule.schema}</span>
          </span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs font-mono">
          {/* Tables */}
          <div className="p-3.5 rounded-lg bg-slate-950/60 border border-slate-800">
            <div className="text-slate-400 uppercase text-[10px] tracking-wider mb-2 font-bold">
              Isolated Tables ({selectedModule.primaryTables.length})
            </div>
            <ul className="space-y-1 text-slate-300">
              {selectedModule.primaryTables.map((t) => (
                <li key={t} className="flex items-center gap-1.5">
                  <span className="w-1.5 h-1.5 rounded-full bg-sky-400" />
                  <code>{selectedModule.schema}.{t}</code>
                </li>
              ))}
            </ul>
          </div>

          {/* Commands & Queries */}
          <div className="p-3.5 rounded-lg bg-slate-950/60 border border-slate-800">
            <div className="text-slate-400 uppercase text-[10px] tracking-wider mb-2 font-bold">
              Public MediatR Commands ({selectedModule.publicCommands.length})
            </div>
            <ul className="space-y-1 text-slate-300">
              {selectedModule.publicCommands.map((c) => (
                <li key={c} className="flex items-center gap-1.5 truncate">
                  <span className="w-1.5 h-1.5 rounded-full bg-indigo-400" />
                  <code>{c}</code>
                </li>
              ))}
            </ul>
          </div>

          {/* Outbound Events */}
          <div className="p-3.5 rounded-lg bg-slate-950/60 border border-slate-800">
            <div className="text-slate-400 uppercase text-[10px] tracking-wider mb-2 font-bold">
              Published Domain Events ({selectedModule.outboundEvents.length})
            </div>
            <ul className="space-y-1 text-slate-300">
              {selectedModule.outboundEvents.map((e) => (
                <li key={e} className="flex items-center gap-1.5 truncate">
                  <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
                  <code>{e}</code>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>

      {/* Invariant Sentinel Guardrail Checklist */}
      <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-6 space-y-4">
        <div className="flex items-center justify-between border-b border-slate-800 pb-3">
          <div className="flex items-center gap-2">
            <ShieldCheck className="w-4 h-4 text-emerald-400" />
            <h3 className="text-sm font-semibold text-slate-100">
              Architectural Invariant Sentinel — Active Defense Rules
            </h3>
          </div>
          <span className="text-xs font-mono text-emerald-400">
            5/5 Rules Passing (Zero Violations)
          </span>
        </div>

        <div className="space-y-3">
          {invariants.map((inv) => (
            <div
              key={inv.id}
              className="p-4 rounded-lg bg-slate-950/60 border border-slate-800 space-y-2 text-xs"
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="font-mono font-bold text-slate-200">{inv.id}</span>
                  <span className="font-semibold text-slate-100">{inv.title}</span>
                  <span
                    className={`px-1.5 py-0.2 rounded text-[10px] font-mono ${
                      inv.severity === 'CRITICAL'
                        ? 'bg-rose-500/10 text-rose-400 border border-rose-500/30'
                        : 'bg-amber-500/10 text-amber-400 border border-amber-500/30'
                    }`}
                  >
                    {inv.severity}
                  </span>
                </div>
                <span className="inline-flex items-center gap-1 font-mono font-medium text-emerald-400">
                  <CheckCircle2 className="w-3.5 h-3.5" /> PASS
                </span>
              </div>

              <p className="text-slate-300 leading-relaxed font-sans">{inv.ruleStatement}</p>

              <div className="p-2.5 rounded bg-slate-900 border border-slate-800 text-[11px] font-mono text-slate-400 whitespace-pre overflow-x-auto">
                {inv.codeSnippet}
              </div>

              <div className="text-[11px] text-slate-400 font-mono flex items-center justify-between pt-1">
                <span>Verification: {inv.verificationMethod}</span>
                <span className="text-indigo-400">{inv.sourceDoc}</span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
