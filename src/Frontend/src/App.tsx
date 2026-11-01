import { useState } from 'react';
import { TASKS_DATA } from './data/tasksData';
import { TopNavigation, ActiveTab } from './components/TopNavigation';
import { PromptCompilerView } from './components/PromptCompiler/PromptCompilerView';
import { FeatureMatrixView } from './components/FeatureMatrix/FeatureMatrixView';
import { TopologySentinelView } from './components/TopologySentinel/TopologySentinelView';
import { SpecInspectorView } from './components/SpecInspector/SpecInspectorView';
import { Toast } from './components/common/Toast';

export default function App() {
  const [activeTab, setActiveTab] = useState<ActiveTab>('prompts');
  const [selectedTaskId, setSelectedTaskId] = useState<string>('RF-CORE-002');
  const [toastMessage, setToastMessage] = useState<string>('');
  const [isToastVisible, setIsToastVisible] = useState<boolean>(false);

  const triggerToast = (message: string) => {
    setToastMessage(message);
    setIsToastVisible(true);
    setTimeout(() => {
      setIsToastVisible(false);
    }, 2800);
  };

  const handleSelectTaskToCompile = (taskId: string) => {
    setSelectedTaskId(taskId);
    setActiveTab('prompts');
    triggerToast(`Loaded slice ${taskId} in Prompt Compiler.`);
  };

  const activeTaskCount = {
    completed: TASKS_DATA.filter((t) => t.status === 'COMPLETED').length,
    inProgress: TASKS_DATA.filter((t) => t.status === 'IN_PROGRESS').length,
    planned: TASKS_DATA.filter((t) => t.status === 'PLANNED').length
  };

  return (
    <div className="min-h-screen bg-[#0B0F19] text-slate-100 flex flex-col font-sans selection:bg-indigo-500/30 selection:text-indigo-200">
      {/* Top Header & Navigation */}
      <TopNavigation
        activeTab={activeTab}
        onTabChange={setActiveTab}
        activeTaskCount={activeTaskCount}
      />

      {/* Main View Port */}
      <main className="flex-1 pb-16">
        {activeTab === 'prompts' && (
          <PromptCompilerView
            tasks={TASKS_DATA}
            selectedTaskId={selectedTaskId}
            onSelectTask={setSelectedTaskId}
            onTriggerToast={triggerToast}
          />
        )}

        {activeTab === 'matrix' && (
          <FeatureMatrixView
            tasks={TASKS_DATA}
            onSelectTaskToCompile={handleSelectTaskToCompile}
          />
        )}

        {activeTab === 'topology' && (
          <TopologySentinelView onTriggerToast={triggerToast} />
        )}

        {activeTab === 'specs' && (
          <SpecInspectorView onTriggerToast={triggerToast} />
        )}
      </main>

      {/* Toast Notification */}
      <Toast message={toastMessage} isVisible={isToastVisible} />
    </div>
  );
}
