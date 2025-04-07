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

  // In-memory cache for data values
  private static dataCache: Record<string, unknown> = {};

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
          const parsedState = JSON.parse(savedState) as {
            flags?: Record<string, boolean>;
            data?: Record<string, unknown>;
          };

          // Extract flags and data from the parsed state
          if (parsedState.flags) {
            this.flagCache = parsedState.flags;
          } else {
            // For backward compatibility with older saves
            // Cast to Record<string, boolean> for type safety
            this.flagCache = parsedState as unknown as Record<string, boolean>;
          }

          // Load data cache if it exists
          if (parsedState.data) {
            this.dataCache = parsedState.data;
          } else {
            this.dataCache = {};
          }
        } catch (e) {
          // If parsing fails, start with empty cache
          console.error('Failed to parse saved game state:', e);
          this.flagCache = {};
          this.dataCache = {};
        }
      } else {
        // No saved state, start with empty cache
        this.flagCache = {};
        this.dataCache = {};
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
      // Create a combined state object with flags and data
      const combinedState = {
        flags: this.flagCache,
        data: this.dataCache,
      };

      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(combinedState));
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
      const importedState = JSON.parse(jsonState) as Record<string, unknown>;

      // Validate the imported state
      if (typeof importedState !== 'object' || importedState === null) {
        console.error('Invalid state format: not an object');
        return false;
      }

      // Reset caches
      this.flagCache = {};
      this.dataCache = {};

      // Handle new format with separate flags and data
      if (importedState.flags && typeof importedState.flags === 'object') {
        // Copy flags (boolean values only)
        const flagsObj = importedState.flags as Record<string, unknown>;
        for (const [key, value] of Object.entries(flagsObj)) {
          if (typeof value === 'boolean') {
            this.flagCache[key] = value;
          }
        }
      }

      // Handle data values
      if (importedState.data && typeof importedState.data === 'object') {
        // Copy all data values
        const dataObj = importedState.data as Record<string, unknown>;
        this.dataCache = { ...dataObj };
      }

      // Handle old format (backward compatibility)
      if (!importedState.flags && !importedState.data) {
        // Cast to Record<string, unknown> to ensure type safety
        const safeImportedState: Record<string, unknown> = importedState;
        for (const [key, value] of Object.entries(safeImportedState)) {
          if (typeof value === 'boolean') {
            this.flagCache[key] = value;
          }
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
   * Get data value
   * @param id Data identifier
   * @returns Data value or undefined if not set
   */
  public static getData(id: string): unknown {
    if (!id) {
      console.warn('SaveManager.getData called with empty id');
      return undefined;
    }

    this.ensureInitialized();

    return this.dataCache[id];
  }

  /**
   * Set data value
   * @param id Data identifier
   * @param value Data value
   * @returns True if set was successful
   */
  public static setData(id: string, value: unknown): boolean {
    if (!id) {
      console.warn('SaveManager.setData called with empty id');
      return false;
    }

    this.ensureInitialized();

    // Update the cache
    this.dataCache[id] = value;

    // Save to localStorage
    return this.saveToStorage();
  }

  /**
   * Remove data
   * @param id Data identifier
   * @returns True if removal was successful
   */
  public static removeData(id: string): boolean {
    if (!id) {
      console.warn('SaveManager.removeData called with empty id');
      return false;
    }

    this.ensureInitialized();

    // Check if data exists
    if (!(id in this.dataCache)) {
      return this.saveToStorage();
    }

    // Remove from cache
    delete this.dataCache[id];

    // Save to localStorage
    return this.saveToStorage();
  }

  /**
   * Get all data
   * @returns Copy of all data
   */
  public static getAllData(): Record<string, unknown> {
    this.ensureInitialized();

    // Return a copy to prevent direct modification
    return { ...this.dataCache };
  }

  /**
   * Export game state as JSON string
   * @returns JSON string containing game state
   */
  public static exportState(): string {
    this.ensureInitialized();

    // Create a combined state object with flags and data
    const combinedState = {
      flags: this.flagCache,
      data: this.dataCache,
    };

    return JSON.stringify(combinedState);
  }
}
