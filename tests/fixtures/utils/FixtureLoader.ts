import { Fixture, FixtureCollection, FixtureLoaderOptions } from '../types';
import { GameStateFixtures } from '../state/GameStateFixtures';
import { SaveDataFixtures } from '../state/SaveDataFixtures';
import { MainSceneFixtures } from '../scenes/MainSceneFixtures';
import { FieldSceneFixtures } from '../scenes/FieldSceneFixtures';
import { AudioFixtures } from '../audio/AudioFixtures';
import { RadioFixtures } from '../audio/RadioFixtures';
import { NarrativeFixtures } from '../narrative/NarrativeFixtures';
import { EventFixtures } from '../narrative/EventFixtures';
import { InventoryFixtures } from '../inventory/InventoryFixtures';
import { ItemFixtures } from '../inventory/ItemFixtures';

/**
 * Fixture collections
 */
const fixtureCollections: Record<string, FixtureCollection<any>> = {
  gameState: GameStateFixtures,
  saveData: SaveDataFixtures,
  mainScene: MainSceneFixtures,
  fieldScene: FieldSceneFixtures,
  audio: AudioFixtures,
  radio: RadioFixtures,
  narrative: NarrativeFixtures,
  events: EventFixtures,
  inventory: InventoryFixtures,
  items: ItemFixtures,
};

/**
 * Default fixture loader options
 */
const defaultOptions: FixtureLoaderOptions = {
  deepClone: true,
  validate: true,
  resolveDependencies: true,
};

/**
 * Fixture loader
 */
export class FixtureLoader {
  /**
   * Load a fixture by collection and ID
   * @param collectionId Collection ID
   * @param fixtureId Fixture ID
   * @param options Loader options
   * @returns Fixture data
   */
  public static load<T>(
    collectionId: string,
    fixtureId: string,
    options?: Partial<FixtureLoaderOptions>
  ): T {
    // Merge options
    const mergedOptions = { ...defaultOptions, ...options };

    // Get collection
    const collection = fixtureCollections[collectionId];
    if (!collection) {
      throw new Error(`Fixture collection not found: ${collectionId}`);
    }

    // Get fixture
    const fixture = collection.fixtures.find(f => f.id === fixtureId);
    if (!fixture) {
      throw new Error(`Fixture not found: ${collectionId}.${fixtureId}`);
    }

    // Clone data
    const data = mergedOptions.deepClone
      ? JSON.parse(JSON.stringify(fixture.data))
      : fixture.data;

    // Validate data
    if (mergedOptions.validate) {
      this.validateFixture(fixture);
    }

    // Resolve dependencies
    if (mergedOptions.resolveDependencies && fixture.metadata?.dependencies) {
      this.resolveDependencies(data, fixture.metadata.dependencies);
    }

    return data as T;
  }

  /**
   * Load multiple fixtures by collection and IDs
   * @param collectionId Collection ID
   * @param fixtureIds Fixture IDs
   * @param options Loader options
   * @returns Array of fixture data
   */
  public static loadMultiple<T>(
    collectionId: string,
    fixtureIds: string[],
    options?: Partial<FixtureLoaderOptions>
  ): T[] {
    return fixtureIds.map(id => this.load<T>(collectionId, id, options));
  }

  /**
   * Load all fixtures from a collection
   * @param collectionId Collection ID
   * @param options Loader options
   * @returns Array of fixture data
   */
  public static loadAll<T>(
    collectionId: string,
    options?: Partial<FixtureLoaderOptions>
  ): T[] {
    // Get collection
    const collection = fixtureCollections[collectionId];
    if (!collection) {
      throw new Error(`Fixture collection not found: ${collectionId}`);
    }

    // Load all fixtures
    return collection.fixtures.map(fixture => this.load<T>(collectionId, fixture.id, options));
  }

  /**
   * Get a fixture by collection and ID without loading it
   * @param collectionId Collection ID
   * @param fixtureId Fixture ID
   * @returns Fixture
   */
  public static getFixture<T>(
    collectionId: string,
    fixtureId: string
  ): Fixture<T> {
    // Get collection
    const collection = fixtureCollections[collectionId];
    if (!collection) {
      throw new Error(`Fixture collection not found: ${collectionId}`);
    }

    // Get fixture
    const fixture = collection.fixtures.find(f => f.id === fixtureId);
    if (!fixture) {
      throw new Error(`Fixture not found: ${collectionId}.${fixtureId}`);
    }

    return fixture as Fixture<T>;
  }

  /**
   * Get all fixtures from a collection
   * @param collectionId Collection ID
   * @returns Array of fixtures
   */
  public static getAllFixtures<T>(collectionId: string): Fixture<T>[] {
    // Get collection
    const collection = fixtureCollections[collectionId];
    if (!collection) {
      throw new Error(`Fixture collection not found: ${collectionId}`);
    }

    return collection.fixtures as Fixture<T>[];
  }

  /**
   * Get a fixture collection
   * @param collectionId Collection ID
   * @returns Fixture collection
   */
  public static getCollection<T>(collectionId: string): FixtureCollection<T> {
    // Get collection
    const collection = fixtureCollections[collectionId];
    if (!collection) {
      throw new Error(`Fixture collection not found: ${collectionId}`);
    }

    return collection as FixtureCollection<T>;
  }

  /**
   * Get all fixture collections
   * @returns Record of fixture collections
   */
  public static getAllCollections(): Record<string, FixtureCollection<any>> {
    return { ...fixtureCollections };
  }

  /**
   * Validate a fixture
   * @param fixture Fixture to validate
   */
  private static validateFixture(fixture: Fixture<any>): void {
    // Check if fixture has data
    if (!fixture.data) {
      throw new Error(`Fixture has no data: ${fixture.id}`);
    }

    // Additional validation could be added here
  }

  /**
   * Resolve dependencies for a fixture
   * @param data Fixture data
   * @param dependencies Dependencies to resolve
   */
  private static resolveDependencies(data: any, dependencies: string[]): void {
    // For each dependency
    for (const dependency of dependencies) {
      // Parse dependency
      const [collectionId, fixtureId] = dependency.split('.');
      if (!collectionId || !fixtureId) {
        throw new Error(`Invalid dependency format: ${dependency}`);
      }

      // Load dependency
      const dependencyData = this.load(collectionId, fixtureId);

      // Merge dependency data
      // This is a simple merge, more complex merging could be implemented
      if (typeof data === 'object' && typeof dependencyData === 'object') {
        Object.assign(data, dependencyData);
      }
    }
  }
}
