import React from 'react';
import { Terminal, LayoutGrid, ShieldCheck, FileText, CheckCircle2 } from 'lucide-react';

export type ActiveTab = 'prompts' | 'matrix' | 'topology' | 'specs';

interface TopNavigationProps {
  activeTab: ActiveTab;
  onTabChange: (tab: ActiveTab) => void;
  activeTaskCount: {
    completed: number;
    inProgress: number;
    planned: number;
  };
}

export const TopNavigation: React.FC<TopNavigationProps> = ({
  activeTab,
  onTabChange,
  activeTaskCount
}) => {
  const tabs = [
    {
      id: 'prompts' as ActiveTab,
      label: 'Antigravity Prompt Compiler',
      shortLabel: 'Prompt Compiler',
      icon: Terminal,
      badge: 'Active SOP'
    },
    {
      id: 'matrix' as ActiveTab,
      label: 'Feature Matrix & Roadmap',
      shortLabel: 'Feature Matrix',
      icon: LayoutGrid,
      badge: `${activeTaskCount.completed}/${activeTaskCount.completed + activeTaskCount.inProgress + activeTaskCount.planned}`
    },
    {
      id: 'topology' as ActiveTab,
      label: 'System Topology & Sentinel',
      shortLabel: 'Topology & Sentinel',
      icon: ShieldCheck,
      badge: '5 Schemas'
    },
    {
      id: 'specs' as ActiveTab,
      label: 'Specification Docs (23)',
      shortLabel: 'Specifications',
      icon: FileText,
      badge: 'SSOT'
    }
  ];

  return (
    <header className="border-b border-slate-800 bg-slate-900/90 backdrop-blur-md sticky top-0 z-40">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Brand & Workspace */}
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-indigo-500 to-sky-500 p-0.5 shadow-md shadow-indigo-500/20">
              <div className="w-full h-full bg-slate-950 rounded-[7px] flex items-center justify-center font-bold text-sky-400 font-mono text-sm">
                RF
              </div>
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="font-semibold text-slate-100 text-base tracking-tight">RunbookForge</span>
                <span className="px-1.5 py-0.5 rounded text-[10px] font-mono font-medium bg-indigo-500/10 text-indigo-400 border border-indigo-500/30">
                  Control Plane
                </span>
              </div>
              <div className="flex items-center gap-1.5 text-xs text-slate-400">
                <span className="font-mono text-slate-400">workspace: EphemeralEngine</span>
                <span>·</span>
                <span className="text-slate-400">.NET 9 + React 19</span>
              </div>
            </div>
          </div>

          {/* Center Navigation Tabs */}
          <nav className="flex items-center gap-1">
            {tabs.map((tab) => {
              const Icon = tab.icon;
              const isActive = activeTab === tab.id;
              return (
                <button
                  key={tab.id}
                  onClick={() => onTabChange(tab.id)}
                  className={`flex items-center gap-2 px-3.5 py-2 rounded-lg text-xs font-medium transition-all ${
                    isActive
                      ? 'bg-indigo-600/15 text-indigo-300 border border-indigo-500/40 shadow-sm'
                      : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60 border border-transparent'
                  }`}
                >
                  <Icon className={`w-4 h-4 ${isActive ? 'text-indigo-400' : 'text-slate-400'}`} />
                  <span className="hidden md:inline">{tab.label}</span>
                  <span className="md:hidden">{tab.shortLabel}</span>
                  <span
                    className={`px-1.5 py-0.2 rounded text-[10px] font-mono ${
                      isActive
                        ? 'bg-indigo-500/20 text-indigo-300'
                        : 'bg-slate-800 text-slate-400'
                    }`}
                  >
                    {tab.badge}
                  </span>
                </button>
              );
            })}
          </nav>

          {/* Right Status Indicator */}
          <div className="hidden lg:flex items-center gap-3">
            <div className="flex items-center gap-2 px-2.5 py-1 rounded-md bg-slate-950/60 border border-slate-800 text-xs">
              <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
              <span className="font-mono text-slate-300">Stack Verified</span>
              <span className="text-slate-600">|</span>
              <span className="text-emerald-400 flex items-center gap-1 font-mono text-[11px]">
                <CheckCircle2 className="w-3 h-3 inline" /> 186/186 Tests Pass
              </span>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

