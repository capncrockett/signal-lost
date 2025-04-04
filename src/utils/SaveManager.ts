/**
 * SaveManager
 *
 * Utility for saving and loading game state using localStorage
 */
export class SaveManager {
  // Storage key for all game flags
  private static readonly STORAGE_KEY = 'signal-lost-game-state';

  // In-memory cache of flags to reduce localStorage access
  private static flagCache: Record<string, boolean> = {};

  // Flag to track if the cache has been initialized
  private static isCacheInitialized = false;

  /**
   * Initialize the flag cache from localStorage
   * @returns True if initialization was successful
   */
  public static initialize(): boolean {
    try {
      // Load flags from localStorage
      const savedState = localStorage.getItem(this.STORAGE_KEY);

      if (savedState) {
        try {
          // Parse the saved state
          this.flagCache = JSON.parse(savedState);
        } catch (e) {
          // If parsing fails, start with empty cache
          console.error('Failed to parse saved game state:', e);
          this.flagCache = {};
        }
      } else {
        // No saved state, start with empty cache
        this.flagCache = {};
      }

      this.isCacheInitialized = true;
      return true;
    } catch (e) {
      // Handle localStorage access errors
      console.error('Failed to initialize SaveManager:', e);
      this.flagCache = {};
      this.isCacheInitialized = false;
      return false;
    }
  }

  /**
   * Ensure the cache is initialized
   */
  private static ensureInitialized(): void {
    if (!this.isCacheInitialized) {
      this.initialize();
    }
  }

  /**
   * Save the current state to localStorage
   * @returns True if save was successful
   */
  private static saveToStorage(): boolean {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.flagCache));
      return true;
    } catch (e) {
      console.error('Failed to save game state:', e);
      return false;
    }
  }

  /**
   * Get the value of a flag
   * @param id Flag identifier
   * @param defaultValue Default value if flag is not set
   * @returns Flag value or default value
   */
  public static getFlag(id: string, defaultValue: boolean = false): boolean {
    if (!id) {
      console.warn('SaveManager.getFlag called with empty id');
      return defaultValue;
    }

    this.ensureInitialized();

    return id in this.flagCache ? this.flagCache[id] : defaultValue;
  }

  /**
   * Set the value of a flag
   * @param id Flag identifier
   * @param value Flag value
   * @returns True if set was successful
   */
  public static setFlag(id: string, value: boolean): boolean {
    if (!id) {
      console.warn('SaveManager.setFlag called with empty id');
      return false;
    }

    this.ensureInitialized();

    // Update the cache
    this.flagCache[id] = value;

    // Save to localStorage
    return this.saveToStorage();
  }

  /**
   * Clear all flags
   * @returns True if clear was successful
   */
  public static clearFlags(): boolean {
    this.ensureInitialized();

    // Clear the cache
    this.flagCache = {};

    // Save to localStorage
    return this.saveToStorage();
  }

  /**
   * Check if a flag exists
   * @param id Flag identifier
   * @returns True if flag exists
   */
  public static hasFlag(id: string): boolean {
    if (!id) {
      return false;
    }

    this.ensureInitialized();

    return id in this.flagCache;
  }

  /**
   * Get all flags
   * @returns Copy of all flags
   */
  public static getAllFlags(): Record<string, boolean> {
    this.ensureInitialized();

    // Return a copy to prevent direct modification
    return { ...this.flagCache };
  }

  /**
   * Remove a specific flag
   * @param id Flag identifier
   * @returns True if removal was successful
   */
  public static removeFlag(id: string): boolean {
    if (!id) {
      console.warn('SaveManager.removeFlag called with empty id');
      return false;
    }

    this.ensureInitialized();

    // Check if flag exists
    if (!(id in this.flagCache)) {
      // Even if the flag doesn't exist, we still save to localStorage
      // to ensure consistency with the test expectations
      return this.saveToStorage();
    }

    // Remove from cache
    delete this.flagCache[id];

    // Save to localStorage
    return this.saveToStorage();
  }

  /**
   * Import game state from JSON string
   * @param jsonState JSON string containing game state
   * @returns True if import was successful
   */
  public static importState(jsonState: string): boolean {
    if (!jsonState) {
      console.warn('SaveManager.importState called with empty state');
      return false;
    }

    try {
      // Parse the JSON state
      const importedState = JSON.parse(jsonState);

      // Validate the imported state
      if (typeof importedState !== 'object' || importedState === null) {
        console.error('Invalid state format: not an object');
        return false;
      }

      // Update the cache
      this.flagCache = {};

      // Copy only boolean values
      for (const [key, value] of Object.entries(importedState)) {
        if (typeof value === 'boolean') {
          this.flagCache[key] = value;
        }
      }

      this.isCacheInitialized = true;

      // Save to localStorage
      return this.saveToStorage();
    } catch (e) {
      console.error('Failed to import game state:', e);
      return false;
    }
  }

  /**
   * Export game state as JSON string
   * @returns JSON string containing game state
   */
  public static exportState(): string {
    this.ensureInitialized();

    return JSON.stringify(this.flagCache);
  }
}
