/**
 * EventHandler utility for processing game events and handling conditional triggers
 */
import { GameEvent } from '../types/event.d';
import { useGameState } from '../context/GameStateContext';
import { useSignalState } from '../context/SignalStateContext';
import { useProgress } from '../context/ProgressContext';
import { useEvent } from '../context/EventContext';

// Define the structure of a condition
export interface EventCondition {
  type: 'has_item' | 'has_signal' | 'progress_min' | 'objective_completed' | 'variable_equals';
  target: string;
  value?: string | number | boolean;
}

// Define the structure of a trigger
export interface EventTrigger {
  id: string;
  conditions?: EventCondition[];
  action: string;
  parameters?: Record<string, unknown>;
}

/**
 * Class to handle event processing and conditional triggers
 */
export class EventHandler {
  private gameState: ReturnType<typeof useGameState>;
  private signalState: ReturnType<typeof useSignalState>;
  private progressState: ReturnType<typeof useProgress>;
  private eventState: ReturnType<typeof useEvent>;
  private triggers: Record<string, EventTrigger> = {};
  private variables: Record<string, unknown> = {};

  /**
   * Create a new EventHandler
   * @param gameState Game state context
   * @param signalState Signal state context
   * @param progressState Progress state context
   * @param eventState Event state context
   */
  constructor(
    gameState: ReturnType<typeof useGameState>,
    signalState: ReturnType<typeof useSignalState>,
    progressState: ReturnType<typeof useProgress>,
    eventState: ReturnType<typeof useEvent>
  ) {
    this.gameState = gameState;
    this.signalState = signalState;
    this.progressState = progressState;
    this.eventState = eventState;
  }

  /**
   * Register a new trigger
   * @param trigger The trigger to register
   */
  public registerTrigger(trigger: EventTrigger): void {
    this.triggers[trigger.id] = trigger;
  }

  /**
   * Register multiple triggers
   * @param triggers The triggers to register
   */
  public registerTriggers(triggers: EventTrigger[]): void {
    triggers.forEach((trigger) => this.registerTrigger(trigger));
  }

  /**
   * Set a variable value
   * @param name Variable name
   * @param value Variable value
   */
  public setVariable(name: string, value: unknown): void {
    this.variables[name] = value;
  }

  /**
   * Get a variable value
   * @param name Variable name
   * @returns The variable value or undefined if not set
   */
  public getVariable(name: string): unknown {
    return this.variables[name];
  }

  /**
   * Check if a condition is met
   * @param condition The condition to check
   * @returns True if the condition is met, false otherwise
   */
  public checkCondition(condition: EventCondition): boolean {
    switch (condition.type) {
      case 'has_item':
        return this.gameState.state.inventory.includes(condition.target);
      case 'has_signal':
        return this.signalState.state.discoveredSignalIds.includes(condition.target);
      case 'progress_min':
        return this.progressState.state.currentProgress >= (condition.value as number);
      case 'objective_completed':
        return this.progressState.state.completedObjectiveIds.includes(condition.target);
      case 'variable_equals':
        return this.variables[condition.target] === condition.value;
      default:
        return false;
    }
  }

  /**
   * Check if all conditions for a trigger are met
   * @param trigger The trigger to check
   * @returns True if all conditions are met, false otherwise
   */
  public checkTriggerConditions(trigger: EventTrigger): boolean {
    if (!trigger.conditions || trigger.conditions.length === 0) {
      return true;
    }

    return trigger.conditions.every((condition) => this.checkCondition(condition));
  }

  /**
   * Process an event and execute any matching triggers
   * @param event The event to process
   * @returns Array of trigger IDs that were executed
   */
  public processEvent(event: GameEvent): string[] {
    const executedTriggers: string[] = [];

    // Mark the event as processed
    this.eventState.markEventProcessed(event.id);

    // Check all triggers
    Object.values(this.triggers).forEach((trigger) => {
      if (this.checkTriggerConditions(trigger)) {
        this.executeTrigger(trigger);
        executedTriggers.push(trigger.id);
      }
    });

    return executedTriggers;
  }

  /**
   * Execute a trigger action
   * @param trigger The trigger to execute
   */
  private executeTrigger(trigger: EventTrigger): void {
    const [actionType, actionTarget] = trigger.action.split(':');

    switch (actionType) {
      case 'dispatch_event':
        this.eventState.dispatchEvent('narrative', {
          type: actionTarget,
          ...trigger.parameters,
        });
        break;
      case 'set_variable':
        if (trigger.parameters && 'value' in trigger.parameters) {
          this.setVariable(actionTarget, trigger.parameters.value);
        }
        break;
      case 'complete_objective':
        this.progressState.completeObjective(actionTarget);
        break;
      case 'discover_signal':
        this.signalState.discoverSignal(actionTarget);
        break;
      case 'set_progress':
        if (trigger.parameters && 'value' in trigger.parameters) {
          this.progressState.setProgress(trigger.parameters.value as number);
        }
        break;
      case 'add_inventory':
        this.gameState.dispatch({
          type: 'ADD_INVENTORY_ITEM',
          payload: actionTarget,
        });
        break;
      default:
        console.warn(`Unknown trigger action: ${trigger.action}`);
    }
  }

  /**
   * Process all pending events
   * @returns Array of event IDs that were processed
   */
  public processPendingEvents(): string[] {
    const pendingEvents = this.eventState.getPendingEvents();
    const processedEvents: string[] = [];

    pendingEvents.forEach((event) => {
      this.processEvent(event);
      processedEvents.push(event.id);
    });

    return processedEvents;
  }

  /**
   * Load triggers from a JSON file
   * @param url URL of the JSON file
   * @returns Promise that resolves when triggers are loaded
   */
  public async loadTriggersFromJson(url: string): Promise<void> {
    try {
      const response = await fetch(url);
      const triggers = await response.json();
      this.registerTriggers(triggers);
    } catch (error) {
      console.error('Error loading triggers:', error);
    }
  }
}

export default EventHandler;
