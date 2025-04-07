/**
 * Test fixture types
 */

/**
 * Base fixture interface
 */
export interface Fixture<T> {
  /**
   * Fixture ID
   */
  id: string;
  
  /**
   * Fixture data
   */
  data: T;
  
  /**
   * Fixture metadata
   */
  metadata?: FixtureMetadata;
}

/**
 * Fixture metadata
 */
export interface FixtureMetadata {
  /**
   * Fixture description
   */
  description?: string;
  
  /**
   * Fixture tags
   */
  tags?: string[];
  
  /**
   * Fixture dependencies
   */
  dependencies?: string[];
  
  /**
   * Fixture author
   */
  author?: string;
  
  /**
   * Fixture creation date
   */
  createdAt?: string;
  
  /**
   * Fixture update date
   */
  updatedAt?: string;
}

/**
 * Fixture collection
 */
export interface FixtureCollection<T> {
  /**
   * Collection ID
   */
  id: string;
  
  /**
   * Collection fixtures
   */
  fixtures: Fixture<T>[];
  
  /**
   * Collection metadata
   */
  metadata?: FixtureMetadata;
}

/**
 * Fixture loader options
 */
export interface FixtureLoaderOptions {
  /**
   * Whether to deep clone fixtures
   */
  deepClone?: boolean;
  
  /**
   * Whether to validate fixtures
   */
  validate?: boolean;
  
  /**
   * Whether to resolve dependencies
   */
  resolveDependencies?: boolean;
}

/**
 * Game state fixture types
 */
export type GameStateFixture = Fixture<Record<string, unknown>>;
export type SaveDataFixture = Fixture<Record<string, unknown>>;

/**
 * Scene fixture types
 */
export type MainSceneFixture = Fixture<Record<string, unknown>>;
export type FieldSceneFixture = Fixture<Record<string, unknown>>;

/**
 * Audio fixture types
 */
export type AudioFixture = Fixture<Record<string, unknown>>;
export type RadioFixture = Fixture<Record<string, unknown>>;

/**
 * Narrative fixture types
 */
export type NarrativeFixture = Fixture<Record<string, unknown>>;
export type EventFixture = Fixture<Record<string, unknown>>;

/**
 * Inventory fixture types
 */
export type InventoryFixture = Fixture<Record<string, unknown>>;
export type ItemFixture = Fixture<Record<string, unknown>>;
