/**
 * Mock AudioManager for testing
 */
export class AudioManager {
  private static instance: AudioManager;
  private masterVolume: number = 0.1;
  private listeners: ((volume: number) => void)[] = [];

  private constructor() {
    // Private constructor to enforce singleton pattern
  }

  /**
   * Get the singleton instance
   */
  public static getInstance(): AudioManager {
    if (!AudioManager.instance) {
      AudioManager.instance = new AudioManager();
    }
    return AudioManager.instance;
  }

  /**
   * Set the master volume
   * @param volume Value between 0 (silent) and 1 (full volume)
   */
  public setMasterVolume(volume: number): void {
    this.masterVolume = Math.max(0, Math.min(1, volume));
    this.notifyListeners();
  }

  /**
   * Get the current master volume
   * @returns Current master volume (0-1)
   */
  public getMasterVolume(): number {
    return this.masterVolume;
  }

  /**
   * Add a volume change listener
   * @param listener Function to call when volume changes
   */
  public addVolumeChangeListener(listener: (volume: number) => void): void {
    this.listeners.push(listener);
    listener(this.masterVolume);
  }

  /**
   * Remove a volume change listener
   * @param listener Function to remove
   */
  public removeVolumeChangeListener(listener: (volume: number) => void): void {
    const index = this.listeners.indexOf(listener);
    if (index !== -1) {
      this.listeners.splice(index, 1);
    }
  }

  /**
   * Notify all listeners of volume change
   */
  private notifyListeners(): void {
    for (const listener of this.listeners) {
      listener(this.masterVolume);
    }
  }

  /**
   * Reset the singleton instance (for testing)
   */
  public static resetInstance(): void {
    AudioManager.instance = null as any;
  }
}
