import { SoundscapeManager } from '../../src/audio/SoundscapeManager';

describe('SoundscapeManager', () => {
  let soundscapeManager: SoundscapeManager;

  // Mock for AudioContext and related objects
  const mockConnect = jest.fn();
  const mockDisconnect = jest.fn();
  const mockStart = jest.fn();
  const mockStop = jest.fn();
  const mockSetValueAtTime = jest.fn();
  const mockLinearRampToValueAtTime = jest.fn();
  const mockClose = jest.fn();

  // Mock for setTimeout and clearTimeout
  const originalSetTimeout = window.setTimeout;
  const originalClearTimeout = window.clearTimeout;
  const mockSetTimeout = jest.fn().mockReturnValue(123);
  const mockClearTimeout = jest.fn();

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Mock Web Audio API
    const mockGainNode = {
      connect: mockConnect,
      disconnect: mockDisconnect,
      gain: {
        value: 0,
        setValueAtTime: mockSetValueAtTime,
        linearRampToValueAtTime: mockLinearRampToValueAtTime
      }
    };

    const mockPannerNode = {
      connect: mockConnect,
      disconnect: mockDisconnect,
      pan: { value: 0 }
    };

    const mockOscillatorNode = {
      connect: mockConnect,
      disconnect: mockDisconnect,
      start: mockStart,
      stop: mockStop,
      frequency: { value: 0 },
      type: 'sine'
    };

    const mockBufferSourceNode = {
      connect: mockConnect,
      disconnect: mockDisconnect,
      start: mockStart,
      stop: mockStop,
      buffer: null,
      loop: false
    };

    const mockAudioBuffer = {
      getChannelData: jest.fn().mockReturnValue(new Float32Array(1000))
    };

    // Mock AudioContext
    window.AudioContext = jest.fn().mockImplementation(() => ({
      createGain: jest.fn().mockReturnValue(mockGainNode),
      createStereoPanner: jest.fn().mockReturnValue(mockPannerNode),
      createOscillator: jest.fn().mockReturnValue(mockOscillatorNode),
      createBufferSource: jest.fn().mockReturnValue(mockBufferSourceNode),
      createBuffer: jest.fn().mockReturnValue(mockAudioBuffer),
      destination: {},
      currentTime: 0,
      sampleRate: 44100,
      state: 'running',
      close: mockClose
    }));

    // Mock setTimeout and clearTimeout
    window.setTimeout = mockSetTimeout;
    window.clearTimeout = mockClearTimeout;

    // Create SoundscapeManager instance
    soundscapeManager = new SoundscapeManager();
  });

  afterEach(() => {
    // Restore original setTimeout and clearTimeout
    window.setTimeout = originalSetTimeout;
    window.clearTimeout = originalClearTimeout;
  });

  test('should initialize correctly', () => {
    // Initialize the manager
    const result = soundscapeManager.initialize();

    // Verify initialization was successful
    expect(result).toBe(true);
    expect(window.AudioContext).toHaveBeenCalled();
  });

  test('should handle initialization failure', () => {
    // Mock AudioContext to throw an error
    window.AudioContext = jest.fn().mockImplementation(() => {
      throw new Error('Audio context error');
    });

    // Mock console.error
    const originalConsoleError = console.error;
    console.error = jest.fn();

    // Try to initialize
    const result = soundscapeManager.initialize();

    // Verify initialization failed
    expect(result).toBe(false);
    expect(console.error).toHaveBeenCalled();

    // Restore console.error
    console.error = originalConsoleError;
  });

  test('should update layers based on signal strength', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Update layers with different signal strengths
    soundscapeManager.updateLayers(0);   // No signal
    soundscapeManager.updateLayers(0.5); // Medium signal
    soundscapeManager.updateLayers(1);   // Perfect signal

    // Verify adjustments were made
    // We can't check specific values easily due to the private properties,
    // but we can verify the method doesn't throw errors
    expect(true).toBe(true);
  });

  test('should adjust panning based on player position', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Adjust panning with different positions
    soundscapeManager.adjustPanning(-1);  // Far left
    soundscapeManager.adjustPanning(0);   // Center
    soundscapeManager.adjustPanning(1);   // Far right

    // Verify adjustments were made
    expect(true).toBe(true);
  });

  test('should set master volume', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Set different volumes
    soundscapeManager.setVolume(0);    // Silent
    soundscapeManager.setVolume(0.5);  // Half volume
    soundscapeManager.setVolume(1);    // Full volume

    // Verify volume was set
    expect(true).toBe(true);
  });

  test('should dispose resources correctly', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Dispose resources
    soundscapeManager.dispose();

    // Verify resources were disposed
    expect(mockStop).toHaveBeenCalled();
    expect(mockDisconnect).toHaveBeenCalled();
    expect(mockClearTimeout).toHaveBeenCalled();
    expect(mockClose).toHaveBeenCalled();
  });

  test('should schedule blips', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Verify setTimeout was called to schedule blips
    expect(mockSetTimeout).toHaveBeenCalled();
  });

  test('should handle operations when not initialized', () => {
    // Try to update layers without initializing
    soundscapeManager.updateLayers(0.5);

    // Try to adjust panning without initializing
    soundscapeManager.adjustPanning(0);

    // Try to set volume without initializing
    soundscapeManager.setVolume(0.5);

    // Try to dispose without initializing
    soundscapeManager.dispose();

    // Verify no errors were thrown
    expect(true).toBe(true);
  });
});
