import React, { useState, useMemo } from 'react';
import {
  CheckCircle2,
  Clock,
  PlayCircle,
  Search,
  ChevronRight,
  Sparkles,
  Calendar
} from 'lucide-react';
import { TaskDefinition, TaskStatus, ModuleCategory } from '../../types/controlPlane';
import { MILESTONES_DATA } from '../../data/tasksData';
import { StatusBadge } from '../common/StatusBadge';

interface FeatureMatrixViewProps {
  tasks: TaskDefinition[];
  onSelectTaskToCompile: (taskId: string) => void;
}

export const FeatureMatrixView: React.FC<FeatureMatrixViewProps> = ({
  tasks,
  onSelectTaskToCompile
}) => {
  const [selectedModule, setSelectedModule] = useState<ModuleCategory | 'ALL'>('ALL');
  const [selectedStatus, setSelectedStatus] = useState<TaskStatus | 'ALL'>('ALL');
  const [searchQuery, setSearchQuery] = useState('');

  const completedCount = useMemo(() => tasks.filter((t) => t.status === 'COMPLETED').length, [tasks]);
  const inProgressCount = useMemo(() => tasks.filter((t) => t.status === 'IN_PROGRESS').length, [tasks]);
  const plannedCount = useMemo(() => tasks.filter((t) => t.status === 'PLANNED').length, [tasks]);
  const blockedCount = useMemo(() => tasks.filter((t) => t.status === 'BLOCKED').length, [tasks]);
  const totalCount = tasks.length;
  const progressPercent = Math.round((completedCount / totalCount) * 100);

  const filteredTasks = useMemo(() => {
    return tasks.filter((task) => {
      if (selectedModule !== 'ALL' && task.module !== selectedModule) return false;
      if (selectedStatus !== 'ALL' && task.status !== selectedStatus) return false;
      if (searchQuery.trim() !== '') {
        const query = searchQuery.toLowerCase();
        const matchesId = task.id.toLowerCase().includes(query);
        const matchesTitle = task.title.toLowerCase().includes(query);
        const matchesLayer = task.layer.toLowerCase().includes(query);
        const matchesSchema = task.schema.toLowerCase().includes(query);
        return matchesId || matchesTitle || matchesLayer || matchesSchema;
      }
      return true;
    });
  }, [tasks, selectedModule, selectedStatus, searchQuery]);

  const modulesList: (ModuleCategory | 'ALL')[] = [
    'ALL',
    'Core',
    'RunbookEngine',
    'TicketIngestion',
    'PatternAnalytics',
    'GapAnalyzer',
    'AICriticAndTraining'
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Metrics Banner */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
          <div className="flex items-center justify-between text-xs text-slate-400 font-mono mb-1">
            <span>COMPLETED</span>
            <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400" />
          </div>
          <div className="text-2xl font-bold font-mono text-emerald-400">{completedCount}</div>
          <div className="text-[11px] text-slate-400 mt-1">Verified with 0 errors</div>
        </div>

        <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
          <div className="flex items-center justify-between text-xs text-slate-400 font-mono mb-1">
            <span>IN PROGRESS</span>
            <PlayCircle className="w-3.5 h-3.5 text-amber-400" />
          </div>
          <div className="text-2xl font-bold font-mono text-amber-400">{inProgressCount}</div>
          <div className="text-[11px] text-slate-400 mt-1">Active slice execution</div>
        </div>

        <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
          <div className="flex items-center justify-between text-xs text-slate-400 font-mono mb-1">
            <span>PLANNED</span>
            <Clock className="w-3.5 h-3.5 text-indigo-400" />
          </div>
          <div className="text-2xl font-bold font-mono text-indigo-400">{plannedCount}</div>
          <div className="text-[11px] text-slate-400 mt-1">Frozen technical specs</div>
        </div>

        <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
          <div className="flex items-center justify-between text-xs text-slate-400 font-mono mb-1">
            <span>TOTAL SYSTEM PROGRESS</span>
            <span className="font-mono text-sky-400">{progressPercent}%</span>
          </div>
          <div className="text-2xl font-bold font-mono text-slate-100">
            {completedCount} <span className="text-sm text-slate-400 font-normal">/ {totalCount}</span>
          </div>
          <div className="w-full bg-slate-800 h-1.5 rounded-full mt-2 overflow-hidden">
            <div
              className="bg-gradient-to-r from-indigo-500 to-emerald-400 h-full rounded-full transition-all duration-500"
              style={{ width: `${progressPercent}%` }}
            />
          </div>
        </div>
      </div>

      {/* Milestone Roadmap Timeline */}
      <div className="rounded-xl border border-slate-800 bg-slate-900/50 p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-sm font-semibold text-slate-200 flex items-center gap-2">
            <Calendar className="w-4 h-4 text-indigo-400" />
            Milestone Delivery Roadmap
          </h3>
          <span className="text-xs font-mono text-slate-400">Target: EphemeralEngine Monolith</span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-5 gap-3">
          {MILESTONES_DATA.map((ms) => {
            const isCompleted = ms.status === 'COMPLETED';
            const isInProgress = ms.status === 'IN_PROGRESS';
            return (
              <div
                key={ms.id}
                className={`p-3.5 rounded-lg border text-left flex flex-col justify-between ${
                  isCompleted
                    ? 'bg-emerald-950/20 border-emerald-500/40 text-emerald-300'
                    : isInProgress
                    ? 'bg-amber-950/20 border-amber-500/40 text-amber-300'
                    : 'bg-slate-950/40 border-slate-800 text-slate-400'
                }`}
              >
                <div>
                  <div className="flex items-center justify-between mb-1.5 font-mono text-xs">
                    <span className="font-bold">{ms.id}</span>
                    <span
                      className={`px-1.5 py-0.2 rounded text-[10px] ${
                        isCompleted
                          ? 'bg-emerald-500/20 text-emerald-400'
                          : isInProgress
                          ? 'bg-amber-500/20 text-amber-400 animate-pulse'
                          : 'bg-slate-800 text-slate-400'
                      }`}
                    >
                      {ms.status}
                    </span>
                  </div>
                  <h4 className="text-xs font-semibold text-slate-200 mb-1">{ms.name}</h4>
                  <p className="text-[11px] text-slate-400 line-clamp-2 leading-relaxed">{ms.scope}</p>
                </div>
                <div className="mt-3 pt-2 border-t border-slate-800/80 flex items-center justify-between text-[10px] font-mono text-slate-400">
                  <span>{ms.tasks.length} tasks</span>
                  <span>{ms.targetDate}</span>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Filter and Search Controls */}
      <div className="flex flex-col md:flex-row items-center justify-between gap-4 bg-slate-900/60 border border-slate-800 rounded-xl p-4">
        {/* Module Selector */}
        <div className="flex flex-wrap items-center gap-1.5 w-full md:w-auto">
          {modulesList.map((m) => (
            <button
              key={m}
              onClick={() => setSelectedModule(m)}
              className={`px-2.5 py-1 rounded-md text-xs font-medium transition-colors ${
                selectedModule === m
                  ? 'bg-indigo-600 text-white'
                  : 'bg-slate-800 text-slate-400 hover:text-slate-200 hover:bg-slate-700'
              }`}
            >
              {m}
            </button>
          ))}
        </div>

        {/* Status Filters & Search Bar */}
        <div className="flex items-center gap-2 w-full md:w-auto">
          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value as TaskStatus | 'ALL')}
            className="bg-slate-950 border border-slate-800 rounded-lg px-2.5 py-1.5 text-xs text-slate-200 focus:outline-none focus:border-indigo-500"
          >
            <option value="ALL">All Statuses</option>
            <option value="COMPLETED">COMPLETED ({completedCount})</option>
            <option value="IN_PROGRESS">IN_PROGRESS ({inProgressCount})</option>
            <option value="PLANNED">PLANNED ({plannedCount})</option>
            <option value="BLOCKED">BLOCKED ({blockedCount})</option>
          </select>

          <div className="relative w-full md:w-64">
            <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search task ID or title..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full bg-slate-950 border border-slate-800 rounded-lg pl-8 pr-3 py-1.5 text-xs text-slate-200 placeholder-slate-500 focus:outline-none focus:border-indigo-500 font-mono"
            />
          </div>
        </div>
      </div>

      {/* Living Task Matrix Table */}
      <div className="rounded-xl border border-slate-800 bg-slate-900/60 overflow-hidden shadow-xl">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse text-xs">
            <thead>
              <tr className="bg-slate-950/80 border-b border-slate-800 text-slate-400 font-mono text-[11px] uppercase tracking-wider">
                <th className="py-3 px-4">Task ID</th>
                <th className="py-3 px-4">Task Title & Scope</th>
                <th className="py-3 px-4">Target Schema</th>
                <th className="py-3 px-4">Layer</th>
                <th className="py-3 px-4">Context Slices</th>
                <th className="py-3 px-4">Status</th>
                <th className="py-3 px-4 text-right">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/80">
              {filteredTasks.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-slate-400">
                    No tasks match the selected filter criteria.
                  </td>
                </tr>
              ) : (
                filteredTasks.map((task) => (
                  <tr
                    key={task.id}
                    className="hover:bg-slate-800/30 transition-colors group"
                  >
                    <td className="py-3.5 px-4 font-mono font-bold text-slate-200">
                      {task.id}
                    </td>
                    <td className="py-3.5 px-4 font-medium text-slate-200 max-w-sm">
                      <div className="text-slate-100 font-medium">{task.title}</div>
                      {task.notes && (
                        <div className="text-[11px] text-slate-400 mt-0.5 font-normal">
                          {task.notes}
                        </div>
                      )}
                    </td>
                    <td className="py-3.5 px-4 font-mono text-slate-400">
                      {task.schema !== 'none' ? (
                        <span className="px-2 py-0.5 rounded bg-slate-800/80 text-sky-400 border border-slate-700/60">
                          {task.schema}
                        </span>
                      ) : (
                        <span className="text-slate-400">shared</span>
                      )}
                    </td>
                    <td className="py-3.5 px-4 font-mono text-slate-400">
                      {task.layer}
                    </td>
                    <td className="py-3.5 px-4 font-mono text-slate-400">
                      <span className="text-indigo-400">{task.contextSpecs.length} specs</span>
                    </td>
                    <td className="py-3.5 px-4">
                      <StatusBadge status={task.status} size="sm" />
                    </td>
                    <td className="py-3.5 px-4 text-right">
                      <button
                        onClick={() => onSelectTaskToCompile(task.id)}
                        className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded bg-indigo-600/10 hover:bg-indigo-600 text-indigo-400 hover:text-white border border-indigo-500/30 hover:border-indigo-600 font-mono text-xs transition-all shadow-sm"
                      >
                        <Sparkles className="w-3 h-3" />
                        <span>Compile Prompt</span>
                        <ChevronRight className="w-3 h-3" />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};
