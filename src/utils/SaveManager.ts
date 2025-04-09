/**
 * SaveManager handles saving and loading game state with support for multiple save slots
 */

// Define the structure of a save file
export interface SaveFile {
  id: string;
  name: string;
  timestamp: number;
  gameState: unknown;
  signalState: unknown;
  eventState: unknown;
  progressState: unknown;
  screenshot?: string; // Base64 encoded screenshot
}

// Define the save manager configuration
interface SaveManagerConfig {
  maxSaveSlots: number;
  autoSaveInterval: number; // in milliseconds, 0 to disable
  savePrefix: string;
}

// Default configuration
const defaultConfig: SaveManagerConfig = {
  maxSaveSlots: 5,
  autoSaveInterval: 5 * 60 * 1000, // 5 minutes
  savePrefix: 'signal-lost-save',
};

// Class to manage save files
export class SaveManager {
  private config: SaveManagerConfig;
  private autoSaveTimer: number | null = null;

  constructor(config: Partial<SaveManagerConfig> = {}) {
    this.config = { ...defaultConfig, ...config };
  }

  /**
   * Get all save files
   */
  public getSaveFiles(): SaveFile[] {
    const saveFiles: SaveFile[] = [];

    // Iterate through localStorage to find save files
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith(this.config.savePrefix)) {
        try {
          const saveData = localStorage.getItem(key);
          if (saveData) {
            const saveFile = JSON.parse(saveData) as SaveFile;
            saveFiles.push(saveFile);
          }
        } catch (error) {
          console.error('Error parsing save file:', error);
        }
      }
    }

    // Sort by timestamp (newest first)
    return saveFiles.sort((a, b) => b.timestamp - a.timestamp);
  }

  /**
   * Save the current game state
   * @param name Name of the save file
   * @param gameState Current game state
   * @param signalState Current signal state
   * @param eventState Current event state
   * @param progressState Current progress state
   * @param screenshot Optional screenshot (base64 encoded)
   * @returns The save file ID
   */
  public saveGame(
    name: string,
    gameState: unknown,
    signalState: unknown,
    eventState: unknown,
    progressState: unknown,
    screenshot?: string
  ): string {
    // Generate a unique ID for this save
    const id = `${Date.now()}-${Math.floor(Math.random() * 1000)}`;

    // Create the save file
    const saveFile: SaveFile = {
      id,
      name,
      timestamp: Date.now(),
      gameState,
      signalState,
      eventState,
      progressState,
      screenshot,
    };

    // Save to localStorage
    localStorage.setItem(`${this.config.savePrefix}-${id}`, JSON.stringify(saveFile));

    // Check if we need to remove old saves
    this.enforceMaxSaveSlots();

    return id;
  }

  /**
   * Load a save file by ID
   * @param id Save file ID
   * @returns The save file or null if not found
   */
  public loadGame(id: string): SaveFile | null {
    try {
      const saveData = localStorage.getItem(`${this.config.savePrefix}-${id}`);
      if (saveData) {
        return JSON.parse(saveData) as SaveFile;
      }
    } catch (error) {
      console.error('Error loading save file:', error);
    }
    return null;
  }

  /**
   * Delete a save file by ID
   * @param id Save file ID
   * @returns True if the save file was deleted
   */
  public deleteSave(id: string): boolean {
    try {
      localStorage.removeItem(`${this.config.savePrefix}-${id}`);
      return true;
    } catch (error) {
      console.error('Error deleting save file:', error);
      return false;
    }
  }

  /**
   * Start auto-saving
   * @param callback Function to call when auto-saving
   */
  public startAutoSave(
    callback: () => {
      gameState: unknown;
      signalState: unknown;
      eventState: unknown;
      progressState: unknown;
    }
  ): void {
    if (this.config.autoSaveInterval <= 0) {
      return;
    }
    // Clear any existing timer
    this.stopAutoSave();

    // Start a new timer
    this.autoSaveTimer = window.setInterval(() => {
      const { gameState, signalState, eventState, progressState } = callback();
      this.saveGame('Auto Save', gameState, signalState, eventState, progressState);
    }, this.config.autoSaveInterval);
  }

  /**
   * Stop auto-saving
   */
  public stopAutoSave(): void {
    if (this.autoSaveTimer !== null) {
      window.clearInterval(this.autoSaveTimer);
      this.autoSaveTimer = null;
    }
  }

  /**
   * Take a screenshot of the game
   * @returns Promise that resolves to a base64 encoded screenshot
   */
  public takeScreenshot(): Promise<string | null> {
    return new Promise((resolve) => {
      try {
        const canvas = document.querySelector('canvas');
        if (canvas) {
          const screenshot = canvas.toDataURL('image/jpeg', 0.5);
          resolve(screenshot);
        } else {
          resolve(null);
        }
      } catch (error) {
        console.error('Error taking screenshot:', error);
        resolve(null);
      }
    });
  }

  /**
   * Enforce the maximum number of save slots
   * Removes the oldest saves if there are too many
   */
  private enforceMaxSaveSlots(): void {
    const saveFiles = this.getSaveFiles();
    if (saveFiles.length > this.config.maxSaveSlots) {
      // Remove the oldest saves
      const filesToRemove = saveFiles.slice(this.config.maxSaveSlots);
      filesToRemove.forEach((file) => {
        this.deleteSave(file.id);
      });
    }
  }
}
