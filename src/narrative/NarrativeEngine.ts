import Phaser from 'phaser';
import { SaveManager } from '../utils/SaveManager';
import { MessageDecoder } from '../utils/MessageDecoder';

/**
 * Interface for narrative event choices
 */
export interface NarrativeChoice {
  text: string;
  outcome: string;
  condition?: string;
}

/**
 * Interface for narrative events
 */
export interface NarrativeEvent {
  id: string;
  message: string;
  choices: NarrativeChoice[];
  condition?: string;
  image?: string;
  audio?: string;
  interference?: number;
}

/**
 * NarrativeEngine
 *
 * Manages narrative events and branching storylines
 */
export class NarrativeEngine {
  // Event emitter for narrative events
  private eventEmitter: Phaser.Events.EventEmitter;

  // Map of narrative events by ID
  private events: Map<string, NarrativeEvent>;

  // Current active event
  private currentEvent: NarrativeEvent | null = null;

  // History of triggered events
  private eventHistory: string[] = [];

  // Flag variables for conditions
  // Variables can be of different types (string, number, boolean, object)
  private variables: Map<string, unknown> = new Map();

  /**
   * Create a new narrative engine
   */
  constructor() {
    this.eventEmitter = new Phaser.Events.EventEmitter();
    this.events = new Map();

    // Initialize SaveManager
    SaveManager.initialize();

    // Load event history from save
    this.loadEventHistory();
  }

  /**
   * Load narrative events from JSON or YAML string
   * @param data JSON or YAML string containing narrative events
   * @returns True if events were loaded successfully
   */
  loadEvents(data: string): boolean {
    try {
      // Parse the data
      const events = JSON.parse(data);

      // Validate and add events
      if (Array.isArray(events)) {
        for (const event of events) {
          this.addEvent(event);
        }
      } else if (typeof events === 'object' && events !== null) {
        this.addEvent(events);
      } else {
        console.error('Invalid event data format');
        return false;
      }

      return true;
    } catch (error) {
      console.error('Failed to parse event data:', error);
      return false;
    }
  }

  /**
   * Add a narrative event
   * @param event Narrative event to add
   * @returns True if the event was added successfully
   */
  addEvent(event: NarrativeEvent): boolean {
    // Validate event
    if (!event.id) {
      console.error('Event must have an ID');
      return false;
    }

    if (!event.message) {
      console.error('Event must have a message');
      return false;
    }

    if (!Array.isArray(event.choices)) {
      event.choices = [];
    }

    // Add event to map
    this.events.set(event.id, event);

    return true;
  }

  /**
   * Trigger a narrative event by ID
   * @param id Event ID
   * @returns True if the event was triggered successfully
   */
  triggerEvent(id: string): boolean {
    // Get event
    const event = this.events.get(id);

    if (!event) {
      console.error(`Event with ID "${id}" not found`);
      return false;
    }

    // Check condition
    if (event.condition && !this.evaluateCondition(event.condition)) {
      console.log(`Event condition not met: ${event.condition}`);
      return false;
    }

    // Set current event
    this.currentEvent = event;

    // Add to history
    this.eventHistory.push(id);
    SaveManager.setFlag(`event_${id}`, true);

    // Emit event
    this.eventEmitter.emit('narrativeEvent', this.processEvent(event));

    return true;
  }

  /**
   * Process an event for display
   * @param event Event to process
   * @returns Processed event with decoded message
   */
  private processEvent(event: NarrativeEvent): NarrativeEvent {
    // Create a copy of the event
    const processedEvent = { ...event };

    // Apply interference to message if specified
    if (typeof event.interference === 'number') {
      processedEvent.message = MessageDecoder.obfuscateMessage(event.message, event.interference);
    }

    // Filter choices based on conditions
    processedEvent.choices = event.choices.filter((choice) => {
      return !choice.condition || this.evaluateCondition(choice.condition);
    });

    return processedEvent;
  }

  /**
   * Make a choice for the current event
   * @param choiceIndex Index of the choice
   * @returns True if the choice was made successfully
   */
  makeChoice(choiceIndex: number): boolean {
    // Check if there is a current event
    if (!this.currentEvent) {
      console.error('No active event');
      return false;
    }

    // Check if the choice index is valid
    if (choiceIndex < 0 || choiceIndex >= this.currentEvent.choices.length) {
      console.error(`Invalid choice index: ${choiceIndex}`);
      return false;
    }

    // Get the choice
    const choice = this.currentEvent.choices[choiceIndex];

    // Check condition
    if (choice.condition && !this.evaluateCondition(choice.condition)) {
      console.error(`Choice condition not met: ${choice.condition}`);
      return false;
    }

    // Save the choice
    SaveManager.setFlag(`choice_${this.currentEvent.id}_${choiceIndex}`, true);

    // Emit choice event
    this.eventEmitter.emit('narrativeChoice', {
      eventId: this.currentEvent.id,
      choiceIndex,
      choice,
    });

    // Trigger outcome if specified
    if (choice.outcome) {
      if (choice.outcome.startsWith('trigger_')) {
        // Trigger another event
        const eventId = choice.outcome.substring(8);
        return this.triggerEvent(eventId);
      } else if (choice.outcome.startsWith('set_')) {
        // Set a variable
        const parts = choice.outcome.substring(4).split('=');
        if (parts.length === 2) {
          this.setVariable(parts[0], parts[1]);
        }
      }
    }

    // Clear current event
    this.currentEvent = null;

    return true;
  }

  /**
   * Evaluate a condition
   * @param condition Condition to evaluate
   * @returns True if the condition is met
   */
  private evaluateCondition(condition: string): boolean {
    // Simple condition evaluation
    // Format: variable_name=value or event_id or !event_id

    // Check for negation
    if (condition.startsWith('!')) {
      return !this.evaluateCondition(condition.substring(1));
    }

    // Check for equality
    if (condition.includes('=')) {
      const [name, value] = condition.split('=');
      return this.getVariable(name) === value;
    }

    // Check for event in history
    if (condition.startsWith('event_')) {
      const eventId = condition.substring(6);
      return this.eventHistory.includes(eventId);
    }

    // Check for flag
    return SaveManager.getFlag(condition);
  }

  /**
   * Set a variable
   * @param name Variable name
   * @param value Variable value
   */
  setVariable(name: string, value: unknown): void {
    this.variables.set(name, value);
    SaveManager.setFlag(`var_${name}`, true);

    // Try to save the value if it's a boolean
    if (typeof value === 'boolean') {
      SaveManager.setFlag(`var_${name}_value`, value);
    }
  }

  /**
   * Get a variable
   * @param name Variable name
   * @returns Variable value or undefined if not set
   */
  getVariable(name: string): unknown {
    return this.variables.get(name);
  }

  /**
   * Check if an event has been triggered
   * @param id Event ID
   * @returns True if the event has been triggered
   */
  hasTriggeredEvent(id: string): boolean {
    return this.eventHistory.includes(id);
  }

  /**
   * Get the current event
   * @returns Current event or null if no event is active
   */
  getCurrentEvent(): NarrativeEvent | null {
    return this.currentEvent ? this.processEvent(this.currentEvent) : null;
  }

  /**
   * Get all events
   * @returns Map of all events
   */
  getAllEvents(): Map<string, NarrativeEvent> {
    return new Map(this.events);
  }

  /**
   * Get event history
   * @returns Array of triggered event IDs
   */
  getEventHistory(): string[] {
    return [...this.eventHistory];
  }

  /**
   * Load event history from save
   */
  private loadEventHistory(): void {
    this.eventHistory = [];

    // Get all flags
    const flags = SaveManager.getAllFlags() || {};

    // Find event flags
    for (const [key, value] of Object.entries(flags)) {
      if (key.startsWith('event_') && value) {
        const eventId = key.substring(6);
        this.eventHistory.push(eventId);
      }
    }
  }

  /**
   * Reset the narrative engine
   */
  reset(): void {
    this.currentEvent = null;
    this.eventHistory = [];
    this.variables.clear();
  }

  /**
   * Add an event listener
   * @param event Event name
   * @param listener Event listener
   */
  on(event: string, listener: (...args: unknown[]) => void): void {
    this.eventEmitter.on(event, listener);
  }

  /**
   * Remove an event listener
   * @param event Event name
   * @param listener Event listener
   */
  off(event: string, listener: (...args: unknown[]) => void): void {
    this.eventEmitter.off(event, listener);
  }
}
