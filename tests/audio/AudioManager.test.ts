import { AudioManager } from '../../src/audio/AudioManager';

describe('AudioManager', () => {
  // Store original AudioManager instance
  let originalInstance: AudioManager | null = null;

  beforeEach(() => {
    // Store the original instance if it exists
    originalInstance = (AudioManager as any).instance || null;

    // Reset the instance before each test
    (AudioManager as any).instance = undefined;
  });

  afterEach(() => {
    // Restore the original instance after each test
    (AudioManager as any).instance = originalInstance;
  });

  test('should create a singleton instance', () => {
    const instance1 = AudioManager.getInstance();
    const instance2 = AudioManager.getInstance();

    // Both instances should be the same object
    expect(instance1).toBe(instance2);
  });

  test('should set and get master volume', () => {
    const audioManager = AudioManager.getInstance();

    // Default volume should be 0.8 (80%)
    expect(audioManager.getMasterVolume()).toBe(0.8);

    // Set volume to 50%
    audioManager.setMasterVolume(0.5);
    expect(audioManager.getMasterVolume()).toBe(0.5);

    // Set volume to 0% (mute)
    audioManager.setMasterVolume(0);
    expect(audioManager.getMasterVolume()).toBe(0);

    // Set volume to 100%
    audioManager.setMasterVolume(1);
    expect(audioManager.getMasterVolume()).toBe(1);
  });

  test('should clamp volume values', () => {
    const audioManager = AudioManager.getInstance();

    // Set volume below 0 (should clamp to 0)
    audioManager.setMasterVolume(-0.5);
    expect(audioManager.getMasterVolume()).toBe(0);

    // Set volume above 1 (should clamp to 1)
    audioManager.setMasterVolume(1.5);
    expect(audioManager.getMasterVolume()).toBe(1);
  });

  test('should notify listeners when volume changes', () => {
    const audioManager = AudioManager.getInstance();
    const listener1 = jest.fn();
    const listener2 = jest.fn();

    // Add listeners
    audioManager.addVolumeChangeListener(listener1);
    audioManager.addVolumeChangeListener(listener2);

    // Listener should be called immediately with current volume
    expect(listener1).toHaveBeenCalledWith(0.8);
    expect(listener2).toHaveBeenCalledWith(0.8);

    // Reset mock calls
    listener1.mockClear();
    listener2.mockClear();

    // Change volume
    audioManager.setMasterVolume(0.5);

    // Both listeners should be notified
    expect(listener1).toHaveBeenCalledWith(0.5);
    expect(listener2).toHaveBeenCalledWith(0.5);
  });

  test('should remove volume change listeners', () => {
    const audioManager = AudioManager.getInstance();
    const listener1 = jest.fn();
    const listener2 = jest.fn();

    // Add listeners
    audioManager.addVolumeChangeListener(listener1);
    audioManager.addVolumeChangeListener(listener2);

    // Reset mock calls after initial notification
    listener1.mockClear();
    listener2.mockClear();

    // Remove first listener
    audioManager.removeVolumeChangeListener(listener1);

    // Change volume
    audioManager.setMasterVolume(0.3);

    // Only second listener should be notified
    expect(listener1).not.toHaveBeenCalled();
    expect(listener2).toHaveBeenCalledWith(0.3);
  });

  test('should handle removing non-existent listeners', () => {
    const audioManager = AudioManager.getInstance();
    const listener = jest.fn();

    // Try to remove a listener that was never added
    audioManager.removeVolumeChangeListener(listener);

    // Should not throw an error
    expect(() => {
      audioManager.removeVolumeChangeListener(listener);
    }).not.toThrow();
  });

  test('should handle multiple volume changes', () => {
    const audioManager = AudioManager.getInstance();
    const listener = jest.fn();

    // Add listener
    audioManager.addVolumeChangeListener(listener);

    // Reset mock calls after initial notification
    listener.mockClear();

    // Change volume multiple times
    audioManager.setMasterVolume(0.1);
    audioManager.setMasterVolume(0.2);
    audioManager.setMasterVolume(0.3);

    // Listener should be called for each change
    expect(listener).toHaveBeenCalledTimes(3);
    expect(listener).toHaveBeenNthCalledWith(1, 0.1);
    expect(listener).toHaveBeenNthCalledWith(2, 0.2);
    expect(listener).toHaveBeenNthCalledWith(3, 0.3);
  });

  test('should maintain separate listeners list for each instance', () => {
    // This is a bit of a contrived test since AudioManager is a singleton,
    // but it's good to verify the behavior if the implementation changes

    // Reset the instance
    (AudioManager as any).instance = undefined;

    const audioManager1 = AudioManager.getInstance();
    const listener1 = jest.fn();

    // Add listener to first instance
    audioManager1.addVolumeChangeListener(listener1);

    // Reset mock calls after initial notification
    listener1.mockClear();

    // Create a new instance (simulating a reset)
    (AudioManager as any).instance = undefined;
    const audioManager2 = AudioManager.getInstance();
    const listener2 = jest.fn();

    // Add listener to second instance
    audioManager2.addVolumeChangeListener(listener2);

    // Reset mock calls after initial notification
    listener2.mockClear();

    // Change volume on second instance
    audioManager2.setMasterVolume(0.5);

    // Only listener2 should be called
    expect(listener1).not.toHaveBeenCalled();
    expect(listener2).toHaveBeenCalledWith(0.5);
  });
});
