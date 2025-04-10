/**
 * Utility for handling conditional event triggers based on game state
 */
import { GameEvent } from '../types/event.d';
import { Signal } from '../types/signal';

/**
 * Condition type for evaluating game state
 */
export type Condition = {
  type: 'equals' | 'notEquals' | 'greaterThan' | 'lessThan' | 'contains' | 'notContains';
  path: string; // Path to the property in the state object (e.g., 'gameState.currentFrequency')
  value: unknown; // Value to compare against
};

/**
 * Trigger definition for conditional event triggering
 */
export type Trigger = {
  id: string;
  conditions: Condition[];
  event: {
    type: 'signal' | 'narrative' | 'system';
    payload: unknown;
  };
  oneTime: boolean; // Whether this trigger should only fire once
  cooldown?: number; // Optional cooldown in milliseconds before this trigger can fire again
};

/**
 * Trigger state for tracking triggered events
 */
export type TriggerState = {
  triggeredIds: string[];
  lastTriggeredTimestamps: Record<string, number>;
};

/**
 * Evaluates a single condition against the provided state
 * @param condition The condition to evaluate
 * @param state The combined state object containing all state contexts
 * @returns True if the condition is met, false otherwise
 */
export const evaluateCondition = (
  condition: Condition,
  state: Record<string, unknown>
): boolean => {
  // Get the value from the state using the path
  const pathParts = condition.path.split('.');
  let currentValue: unknown = state;

  for (const part of pathParts) {
    if (currentValue === null || currentValue === undefined || typeof currentValue !== 'object') {
      return false;
    }
    currentValue = (currentValue as Record<string, unknown>)[part];
  }

  // Evaluate the condition based on its type
  switch (condition.type) {
    case 'equals':
      return currentValue === condition.value;
    case 'notEquals':
      return currentValue !== condition.value;
    case 'greaterThan':
      return typeof currentValue === 'number' && typeof condition.value === 'number'
        ? currentValue > condition.value
        : false;
    case 'lessThan':
      return typeof currentValue === 'number' && typeof condition.value === 'number'
        ? currentValue < condition.value
        : false;
    case 'contains':
      if (Array.isArray(currentValue)) {
        return currentValue.includes(condition.value);
      }
      if (typeof currentValue === 'string' && typeof condition.value === 'string') {
        return currentValue.includes(condition.value);
      }
      return false;
    case 'notContains':
      if (Array.isArray(currentValue)) {
        return !currentValue.includes(condition.value);
      }
      if (typeof currentValue === 'string' && typeof condition.value === 'string') {
        return !currentValue.includes(condition.value);
      }
      return false;
    default:
      return false;
  }
};

/**
 * Evaluates all conditions for a trigger
 * @param trigger The trigger to evaluate
 * @param state The combined state object containing all state contexts
 * @returns True if all conditions are met, false otherwise
 */
export const evaluateTrigger = (trigger: Trigger, state: Record<string, unknown>): boolean => {
  return trigger.conditions.every((condition) => evaluateCondition(condition, state));
};

/**
 * Checks if a trigger is on cooldown
 * @param trigger The trigger to check
 * @param triggerState The current trigger state
 * @returns True if the trigger is on cooldown, false otherwise
 */
export const isTriggerOnCooldown = (trigger: Trigger, triggerState: TriggerState): boolean => {
  if (!trigger.cooldown) {
    return false;
  }

  const lastTriggered = triggerState.lastTriggeredTimestamps[trigger.id];
  if (!lastTriggered) {
    return false;
  }

  const now = Date.now();
  return now - lastTriggered < trigger.cooldown;
};

/**
 * Checks if a one-time trigger has already been triggered
 * @param trigger The trigger to check
 * @param triggerState The current trigger state
 * @returns True if the trigger has already been triggered, false otherwise
 */
export const hasOneTimeTriggerFired = (trigger: Trigger, triggerState: TriggerState): boolean => {
  return trigger.oneTime && triggerState.triggeredIds.includes(trigger.id);
};

/**
 * Processes all triggers against the current state
 * @param triggers Array of triggers to evaluate
 * @param state The combined state object containing all state contexts
 * @param triggerState The current trigger state
 * @returns Array of events that should be dispatched
 */
export const processTriggers = (
  triggers: Trigger[],
  state: Record<string, unknown>,
  triggerState: TriggerState
): GameEvent[] => {
  const now = Date.now();
  const eventsToDispatch: GameEvent[] = [];

  for (const trigger of triggers) {
    // Skip if one-time trigger has already fired
    if (hasOneTimeTriggerFired(trigger, triggerState)) {
      continue;
    }

    // Skip if trigger is on cooldown
    if (isTriggerOnCooldown(trigger, triggerState)) {
      continue;
    }

    // Evaluate trigger conditions
    if (evaluateTrigger(trigger, state)) {
      // Create event
      const event: GameEvent = {
        id: `${trigger.event.type}_${now}_${Math.floor(Math.random() * 1000)}`,
        type: trigger.event.type,
        payload: trigger.event.payload,
        timestamp: now,
      };

      eventsToDispatch.push(event);

      // Update trigger state
      triggerState.triggeredIds.push(trigger.id);
      triggerState.lastTriggeredTimestamps[trigger.id] = now;
    }
  }

  return eventsToDispatch;
};

/**
 * Creates a signal-specific trigger for detecting signals at specific frequencies
 * @param id Unique identifier for the trigger
 * @param frequency The frequency to detect
 * @param signalData The signal data to dispatch when triggered
 * @param oneTime Whether this trigger should only fire once
 * @param cooldown Optional cooldown in milliseconds
 * @returns A trigger configuration
 */
export const createFrequencyTrigger = (
  id: string,
  frequency: number,
  signalData: Omit<Signal, 'id' | 'timestamp'>,
  oneTime = true,
  cooldown?: number
): Trigger => {
  return {
    id,
    conditions: [
      {
        type: 'equals',
        path: 'gameState.currentFrequency',
        value: frequency,
      },
      {
        type: 'equals',
        path: 'gameState.isRadioOn',
        value: true,
      },
    ],
    event: {
      type: 'signal',
      payload: {
        ...signalData,
        frequency,
      },
    },
    oneTime,
    cooldown,
  };
};

/**
 * Creates a progress-based trigger that fires when a certain progress level is reached
 * @param id Unique identifier for the trigger
 * @param progressLevel The progress level required to trigger
 * @param eventPayload The event payload to dispatch
 * @param oneTime Whether this trigger should only fire once
 * @returns A trigger configuration
 */
export const createProgressTrigger = (
  id: string,
  progressLevel: number,
  eventPayload: unknown,
  oneTime = true
): Trigger => {
  return {
    id,
    conditions: [
      {
        type: 'greaterThan',
        path: 'progressState.currentProgress',
        value: progressLevel - 0.1, // Use a small offset to handle floating point comparison
      },
    ],
    event: {
      type: 'narrative',
      payload: eventPayload,
    },
    oneTime,
  };
};

/**
 * Creates a location-based trigger that fires when the player enters a specific location
 * @param id Unique identifier for the trigger
 * @param location The location that triggers the event
 * @param eventPayload The event payload to dispatch
 * @param oneTime Whether this trigger should only fire once
 * @returns A trigger configuration
 */
export const createLocationTrigger = (
  id: string,
  location: string,
  eventPayload: unknown,
  oneTime = true
): Trigger => {
  return {
    id,
    conditions: [
      {
        type: 'equals',
        path: 'gameState.currentLocation',
        value: location,
      },
    ],
    event: {
      type: 'narrative',
      payload: eventPayload,
    },
    oneTime,
  };
};

/**
 * Creates an inventory-based trigger that fires when the player has a specific item
 * @param id Unique identifier for the trigger
 * @param item The item required in inventory
 * @param eventPayload The event payload to dispatch
 * @param oneTime Whether this trigger should only fire once
 * @returns A trigger configuration
 */
export const createInventoryTrigger = (
  id: string,
  item: string,
  eventPayload: unknown,
  oneTime = true
): Trigger => {
  return {
    id,
    conditions: [
      {
        type: 'contains',
        path: 'gameState.inventory',
        value: item,
      },
    ],
    event: {
      type: 'narrative',
      payload: eventPayload,
    },
    oneTime,
  };
};

/**
 * Initial trigger state
 */
export const initialTriggerState: TriggerState = {
  triggeredIds: [],
  lastTriggeredTimestamps: {},
};
