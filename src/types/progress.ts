/**
 * Types for game progress tracking
 */

/**
 * Objective represents a game objective that the player can complete
 */
export interface Objective {
  id: string;
  title: string;
  description: string;
  isCompleted: boolean;
  requiredProgress: number;
  completedAt?: number;
  dependsOn?: string[]; // IDs of objectives that must be completed first
}

/**
 * Progress state for tracking game progress and objectives
 */
export interface ProgressState {
  currentProgress: number;
  objectives: Record<string, Objective>;
  completedObjectiveIds: string[];
  lastCompletedObjectiveId: string | null;
  lastCompletedTimestamp: number | null;
}

/**
 * Initial state for progress tracking
 */
export const initialProgressState: ProgressState = {
  currentProgress: 0,
  objectives: {},
  completedObjectiveIds: [],
  lastCompletedObjectiveId: null,
  lastCompletedTimestamp: null,
};
