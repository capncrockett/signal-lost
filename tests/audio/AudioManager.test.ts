import { AudioManager } from '../../src/audio/AudioManager';

describe('AudioManager', () => {
  let audioManager: AudioManager;

  beforeEach(() => {
    // Reset the singleton instance before each test
    // This is a bit of a hack, but necessary for testing singletons
    // @ts-expect-error - accessing private property for testing
    AudioManager.instance = undefined;
    audioManager = AudioManager.getInstance();
  });

  test('getInstance returns the same instance', () => {
    const instance1 = AudioManager.getInstance();
    const instance2 = AudioManager.getInstance();

    expect(instance1).toBe(instance2);
  });

  test('initializes with default volume', () => {
    expect(audioManager.getMasterVolume()).toBe(0.8);
  });

  test('setMasterVolume updates volume', () => {
    audioManager.setMasterVolume(0.5);
    expect(audioManager.getMasterVolume()).toBe(0.5);
  });

  test('setMasterVolume clamps values to 0-1 range', () => {
    // Test with value below 0
    audioManager.setMasterVolume(-0.5);
    expect(audioManager.getMasterVolume()).toBe(0);

    // Test with value above 1
    audioManager.setMasterVolume(1.5);
    expect(audioManager.getMasterVolume()).toBe(1);
  });

  test('addVolumeChangeListener adds listener and calls it immediately', () => {
    const listener = jest.fn();

    audioManager.addVolumeChangeListener(listener);

    // Should be called immediately with current volume
    expect(listener).toHaveBeenCalledWith(0.8);

    // Change volume and verify listener is called
    audioManager.setMasterVolume(0.6);
    expect(listener).toHaveBeenCalledWith(0.6);
  });

  test('removeVolumeChangeListener removes the listener', () => {
    const listener = jest.fn();

    // Add listener
    audioManager.addVolumeChangeListener(listener);

    // Clear mock to ignore initial call
    listener.mockClear();

    // Remove listener
    audioManager.removeVolumeChangeListener(listener);

    // Change volume
    audioManager.setMasterVolume(0.4);

    // Listener should not be called
    expect(listener).not.toHaveBeenCalled();
  });

  test('multiple listeners are notified of volume changes', () => {
    const listener1 = jest.fn();
    const listener2 = jest.fn();

    // Add listeners
    audioManager.addVolumeChangeListener(listener1);
    audioManager.addVolumeChangeListener(listener2);

    // Clear mocks to ignore initial calls
    listener1.mockClear();
    listener2.mockClear();

    // Change volume
    audioManager.setMasterVolume(0.3);

    // Both listeners should be called
    expect(listener1).toHaveBeenCalledWith(0.3);
    expect(listener2).toHaveBeenCalledWith(0.3);
  });

  test('removing one listener does not affect others', () => {
    const listener1 = jest.fn();
    const listener2 = jest.fn();

    // Add listeners
    audioManager.addVolumeChangeListener(listener1);
    audioManager.addVolumeChangeListener(listener2);

    // Clear mocks to ignore initial calls
    listener1.mockClear();
    listener2.mockClear();

    // Remove first listener
    audioManager.removeVolumeChangeListener(listener1);

    // Change volume
    audioManager.setMasterVolume(0.2);

    // Only second listener should be called
    expect(listener1).not.toHaveBeenCalled();
    expect(listener2).toHaveBeenCalledWith(0.2);
  });
});
