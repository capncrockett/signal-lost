/**
 * Types for narrative events
 */

/**
 * Narrative event data
 */
export interface NarrativeEventData {
  id: string;
  type: string;
  text: string;
  choices?: NarrativeChoiceData[];
}

/**
 * Narrative choice data
 */
export interface NarrativeChoiceData {
  id: string;
  text: string;
  nextEvent?: string;
}

/**
 * Narrative choice result data
 */
export interface NarrativeChoiceResultData {
  eventId: string;
  choice: NarrativeChoiceData;
}

/**
 * Location discovery data
 */
export interface LocationDiscoveryData {
  id: string;
  x: number;
  y: number;
  name?: string;
}
