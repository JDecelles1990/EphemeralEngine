import React, { useState, useMemo } from 'react';
import {
  FileText,
  Search,
  CheckCircle2,
  Copy
} from 'lucide-react';
import { SPECS_DATA } from '../../data/specsData';
import { SpecDocument } from '../../types/controlPlane';

interface SpecInspectorViewProps {
  onTriggerToast: (message: string) => void;
}

export const SpecInspectorView: React.FC<SpecInspectorViewProps> = ({ onTriggerToast }) => {
  const [selectedCategory, setSelectedCategory] = useState<'All' | 'System' | 'Standards' | 'Database'>('All');
  const [selectedDocId, setSelectedDocId] = useState<string>('00-MASTER-INDEX');
  const [searchQuery, setSearchQuery] = useState('');

  const filteredDocs = useMemo(() => {
    return SPECS_DATA.filter((doc) => {
      if (selectedCategory !== 'All' && doc.category !== selectedCategory) return false;
      if (searchQuery.trim() !== '') {
        const query = searchQuery.toLowerCase();
        return (
          doc.id.toLowerCase().includes(query) ||
          doc.title.toLowerCase().includes(query) ||
          doc.docPath.toLowerCase().includes(query) ||
          doc.summary.toLowerCase().includes(query)
        );
      }
      return true;
    });
  }, [selectedCategory, searchQuery]);

  const activeDoc: SpecDocument = useMemo(() => {
    return SPECS_DATA.find((d) => d.id === selectedDocId) || SPECS_DATA[0];
  }, [selectedDocId]);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      {/* Top Banner */}
      <div className="rounded-xl border border-slate-800 bg-gradient-to-r from-slate-900 via-indigo-950/20 to-slate-900 p-6 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 mb-2">
            <span className="p-1 rounded bg-indigo-500/10 text-indigo-400 border border-indigo-500/30">
              <FileText className="w-4 h-4" />
            </span>
            <h2 className="text-lg font-semibold text-slate-100">Specification & Contract Inspector</h2>
            <span className="px-2 py-0.5 rounded text-[11px] font-mono bg-indigo-500/10 text-indigo-400 border border-indigo-500/30">
              23 Frozen Specifications
            </span>
          </div>
          <p className="text-xs text-slate-400 max-w-2xl leading-relaxed">
            Direct access to the 23 technical specifications governing RunbookForge. Every architectural contract, database schema DDL, language guideline, and inter-module event schema is indexed as Single Source of Truth (SSOT).
          </p>
        </div>

        <button
          onClick={async () => {
            await navigator.clipboard.writeText(activeDoc.docPath);
            onTriggerToast(`Copied spec path: ${activeDoc.docPath}`);
          }}
          className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold shadow-lg shadow-indigo-600/30 transition-all shrink-0"
        >
          <Copy className="w-4 h-4" />
          <span>Copy Doc Path</span>
        </button>
      </div>

      {/* Filter and Search Bar */}
      <div className="flex flex-col md:flex-row items-center justify-between gap-4 bg-slate-900/60 border border-slate-800 rounded-xl p-4">
        <div className="flex items-center gap-1.5 w-full md:w-auto">
          {(['All', 'System', 'Standards', 'Database'] as const).map((cat) => (
            <button
              key={cat}
              onClick={() => setSelectedCategory(cat)}
              className={`px-3 py-1 rounded-md text-xs font-medium transition-colors ${
                selectedCategory === cat
                  ? 'bg-indigo-600 text-white'
                  : 'bg-slate-800 text-slate-400 hover:text-slate-200 hover:bg-slate-700'
              }`}
            >
              {cat}
            </button>
          ))}
        </div>

        <div className="relative w-full md:w-72">
          <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search specifications..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full bg-slate-950 border border-slate-800 rounded-lg pl-8 pr-3 py-1.5 text-xs text-slate-200 placeholder-slate-500 focus:outline-none focus:border-indigo-500 font-mono"
          />
        </div>
      </div>

      {/* Main Two-Column Reader */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left List of Specifications (5 cols) */}
        <div className="lg:col-span-5 rounded-xl border border-slate-800 bg-slate-900/60 p-4 space-y-2 max-h-[680px] overflow-y-auto">
          <div className="flex items-center justify-between mb-2 text-xs font-mono uppercase tracking-wider text-slate-400">
            <span>Documents ({filteredDocs.length})</span>
            <span>Category</span>
          </div>

          {filteredDocs.map((doc) => {
            const isSelected = doc.id === activeDoc.id;
            return (
              <div
                key={doc.id}
                onClick={() => setSelectedDocId(doc.id)}
                className={`p-3 rounded-lg border text-left cursor-pointer transition-all ${
                  isSelected
                    ? 'bg-indigo-950/40 border-indigo-500 shadow-md shadow-indigo-950/50'
                    : 'bg-slate-950/40 border-slate-800/80 hover:border-slate-700 hover:bg-slate-800/30'
                }`}
              >
                <div className="flex items-center justify-between gap-2 mb-1">
                  <span className="font-mono text-xs font-bold text-slate-200 truncate">
                    {doc.id}
                  </span>
                  <span
                    className={`px-1.5 py-0.2 rounded text-[10px] font-mono ${
                      doc.category === 'System'
                        ? 'bg-indigo-500/10 text-indigo-400'
                        : doc.category === 'Standards'
                        ? 'bg-sky-500/10 text-sky-400'
                        : 'bg-amber-500/10 text-amber-400'
                    }`}
                  >
                    {doc.category}
                  </span>
                </div>
                <h4 className="text-xs font-semibold text-slate-200 line-clamp-1">{doc.title}</h4>
                <p className="text-[11px] text-slate-400 line-clamp-1 mt-0.5 font-mono">{doc.docPath}</p>
              </div>
            );
          })}
        </div>

        {/* Right Detail Pane (7 cols) */}
        <div className="lg:col-span-7 rounded-xl border border-slate-800 bg-slate-900/60 p-6 space-y-5">
          {/* Header */}
          <div className="border-b border-slate-800 pb-4">
            <div className="flex items-center justify-between gap-2 mb-1">
              <span className="text-xs font-mono px-2 py-0.5 rounded bg-indigo-500/20 text-indigo-300 font-bold">
                {activeDoc.id}
              </span>
              <span className="text-xs font-mono text-slate-400">{activeDoc.docPath}</span>
            </div>
            <h3 className="text-xl font-bold text-slate-100">{activeDoc.title}</h3>
            <div className="mt-2 text-xs font-mono text-sky-400">
              SSOT Domain: <span className="text-slate-300">{activeDoc.ssotDomain}</span>
            </div>
          </div>

          {/* Summary Box */}
          <div className="p-4 rounded-lg bg-slate-950/60 border border-slate-800 text-xs text-slate-300 leading-relaxed">
            <span className="font-semibold text-slate-200 block mb-1">Summary:</span>
            {activeDoc.summary}
          </div>

          {/* Key Directives */}
          <div className="space-y-2">
            <h4 className="text-xs font-mono uppercase tracking-wider text-slate-400 font-bold">
              Key Architectural Directives
            </h4>
            <div className="space-y-1.5">
              {activeDoc.keyDirectives.map((directive, idx) => (
                <div
                  key={idx}
                  className="flex items-start gap-2 text-xs text-slate-300 p-2 rounded bg-slate-950/40 border border-slate-800/80"
                >
                  <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400 shrink-0 mt-0.5" />
                  <span>{directive}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Excerpt */}
          <div className="space-y-2">
            <h4 className="text-xs font-mono uppercase tracking-wider text-slate-400 font-bold">
              Specification Header Excerpt
            </h4>
            <pre className="p-3.5 rounded-lg bg-slate-950 border border-slate-800 text-xs font-mono text-slate-300 overflow-x-auto whitespace-pre">
              {activeDoc.contentSample}
            </pre>
          </div>
        </div>
      </div>
    </div>
  );
};
