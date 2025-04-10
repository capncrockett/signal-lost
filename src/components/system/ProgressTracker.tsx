import React, { useEffect } from 'react';
import { useProgress } from '../../context/ProgressContext';
import { useEvent } from '../../context/EventContext';
import { useSignalState } from '../../context/SignalStateContext';
import { Objective } from '../../types/progress';

interface ProgressTrackerProps {
  onProgressChanged?: (progress: number) => void;
  onObjectiveCompleted?: (objective: Objective) => void;
  autoUpdateProgress?: boolean;
}

/**
 * ProgressTracker component that manages game progress tracking
 * This component doesn't render anything but provides functionality
 * for tracking and updating game progress
 */
const ProgressTracker: React.FC<ProgressTrackerProps> = ({
  onProgressChanged,
  onObjectiveCompleted,
  autoUpdateProgress = true,
}) => {
  const {
    state: progressState,
    setProgress,
    getObjectiveById,
    getCompletedObjectives,
    getPendingObjectives,
    completeObjective,
  } = useProgress();
  
  const { state: signalState } = useSignalState();
  const { dispatchEvent } = useEvent();

  // Track progress changes
  useEffect(() => {
    if (onProgressChanged) {
      onProgressChanged(progressState.currentProgress);
    }
  }, [progressState.currentProgress, onProgressChanged]);

  // Track completed objectives
  useEffect(() => {
    if (!progressState.lastCompletedObjectiveId) return;
    
    const objective = getObjectiveById(progressState.lastCompletedObjectiveId);
    if (!objective) return;
    
    // Notify about the completed objective
    if (onObjectiveCompleted) {
      onObjectiveCompleted(objective);
    }
    
    // Dispatch an event for the completed objective
    dispatchEvent('narrative', {
      type: 'objective_completed',
      objectiveId: objective.id,
      title: objective.title,
      timestamp: Date.now(),
    });
  }, [
    progressState.lastCompletedObjectiveId,
    progressState.lastCompletedTimestamp,
    getObjectiveById,
    dispatchEvent,
    onObjectiveCompleted,
  ]);

  // Automatically update progress based on completed objectives and discovered signals
  useEffect(() => {
    if (!autoUpdateProgress) return;
    
    // Calculate progress based on completed objectives
    const completedObjectives = getCompletedObjectives();
    const pendingObjectives = getPendingObjectives();
    const totalObjectives = completedObjectives.length + pendingObjectives.length;
    
    // Calculate progress based on discovered signals
    const discoveredSignals = signalState.discoveredSignalIds.length;
    const totalSignals = Object.keys(signalState.signals).length;
    
    // Combine both factors to calculate overall progress
    let newProgress = 0;
    
    if (totalObjectives > 0) {
      newProgress += (completedObjectives.length / totalObjectives) * 0.7; // 70% weight for objectives
    }
    
    if (totalSignals > 0) {
      newProgress += (discoveredSignals / totalSignals) * 0.3; // 30% weight for signals
    }
    
    // Only update if progress has changed significantly
    if (Math.abs(newProgress - progressState.currentProgress) > 0.01) {
      setProgress(newProgress);
    }
  }, [
    autoUpdateProgress,
    progressState.completedObjectiveIds,
    signalState.discoveredSignalIds,
    getCompletedObjectives,
    getPendingObjectives,
    setProgress,
    progressState.currentProgress,
    signalState.signals,
  ]);

  // This component doesn't render anything
  return null;
};

export default ProgressTracker;
