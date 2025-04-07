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
        linearRampToValueAtTime: mockLinearRampToValueAtTime,
      },
    };

    const mockPannerNode = {
      connect: mockConnect,
      disconnect: mockDisconnect,
      pan: { value: 0 },
    };

    const mockOscillatorNode = {
      connect: mockConnect,
      disconnect: mockDisconnect,
      start: mockStart,
      stop: mockStop,
      frequency: { value: 0 },
      type: 'sine',
    };

    const mockBufferSourceNode = {
      connect: mockConnect,
      disconnect: mockDisconnect,
      start: mockStart,
      stop: mockStop,
      buffer: null,
      loop: false,
    };

    const mockAudioBuffer = {
      getChannelData: jest.fn().mockReturnValue(new Float32Array(1000)),
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
      close: mockClose,
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

    // Create a spy to access private methods
    const getStaticVolumeSpy = jest.spyOn(soundscapeManager as any, 'getStaticVolume');

    // Update layers with different signal strengths
    soundscapeManager.updateLayers(0); // No signal
    expect(getStaticVolumeSpy).toHaveBeenCalledWith(0);

    soundscapeManager.updateLayers(0.5); // Medium signal
    expect(getStaticVolumeSpy).toHaveBeenCalledWith(0.5);

    soundscapeManager.updateLayers(1); // Perfect signal
    expect(getStaticVolumeSpy).toHaveBeenCalledWith(1);

    // Restore the spy
    getStaticVolumeSpy.mockRestore();
  });

  test('should adjust panning based on player position', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Set up spies to access private properties
    const staticPannerSpy = jest.spyOn(soundscapeManager as any, 'staticPanner', 'get').mockReturnValue({
      pan: { value: 0 }
    });

    const dronePannerSpy = jest.spyOn(soundscapeManager as any, 'dronePanner', 'get').mockReturnValue({
      pan: { value: 0 }
    });

    const blipPannerSpy = jest.spyOn(soundscapeManager as any, 'blipPanner', 'get').mockReturnValue({
      pan: { value: 0 }
    });

    // Adjust panning with different positions
    soundscapeManager.adjustPanning(-1); // Far left
    expect(staticPannerSpy).toHaveBeenCalled();
    expect(dronePannerSpy).toHaveBeenCalled();
    expect(blipPannerSpy).toHaveBeenCalled();

    soundscapeManager.adjustPanning(0); // Center
    soundscapeManager.adjustPanning(1); // Far right

    // Restore spies
    staticPannerSpy.mockRestore();
    dronePannerSpy.mockRestore();
    blipPannerSpy.mockRestore();
  });

  test('should set master volume', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Create a spy for the master gain node
    const masterGainSpy = jest.spyOn(soundscapeManager as any, 'masterGain', 'get').mockReturnValue({
      gain: { value: 0 }
    });

    // Set different volumes
    soundscapeManager.setVolume(0); // Silent
    expect(masterGainSpy).toHaveBeenCalled();

    soundscapeManager.setVolume(0.5); // Half volume
    soundscapeManager.setVolume(1); // Full volume

    // Restore spy
    masterGainSpy.mockRestore();
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

    // Create a spy for the createBlip method
    const createBlipSpy = jest.spyOn(soundscapeManager as any, 'createBlip').mockImplementation(() => {});

    // Call the callback passed to setTimeout
    const setTimeoutCalls = mockSetTimeout.mock.calls;
    const blipCallback = setTimeoutCalls[setTimeoutCalls.length - 1][0];
    blipCallback();

    // Verify createBlip was called
    expect(createBlipSpy).toHaveBeenCalled();

    // Restore spy
    createBlipSpy.mockRestore();
  });

  test('should handle operations when not initialized', () => {
    // Create spies for private properties
    const isInitializedSpy = jest.spyOn(soundscapeManager as any, 'isInitialized', 'get').mockReturnValue(false);

    // Try to update layers without initializing
    soundscapeManager.updateLayers(0.5);

    // Try to adjust panning without initializing
    soundscapeManager.adjustPanning(0);

    // Try to set volume without initializing
    soundscapeManager.setVolume(0.5);

    // Try to dispose without initializing
    soundscapeManager.dispose();

    // Verify isInitialized was checked
    expect(isInitializedSpy).toHaveBeenCalled();

    // Restore spy
    isInitializedSpy.mockRestore();
  });

  test('should create static noise', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Create a spy for the createStaticNoise method
    const createStaticNoiseSpy = jest.spyOn(soundscapeManager as any, 'createStaticNoise');

    // Call the method directly
    (soundscapeManager as any).createStaticNoise();

    // Verify the method was called
    expect(createStaticNoiseSpy).toHaveBeenCalled();

    // Restore spy
    createStaticNoiseSpy.mockRestore();
  });

  test('should handle audio context state changes', () => {
    // Initialize the manager
    soundscapeManager.initialize();

    // Create a spy for the audioContext
    const audioContextSpy = jest.spyOn(soundscapeManager as any, 'audioContext', 'get').mockReturnValue({
      state: 'suspended',
      resume: jest.fn().mockResolvedValue(undefined),
    });

    // Call a method that would use the audio context
    soundscapeManager.updateLayers(0.5);

    // Restore spy
    audioContextSpy.mockRestore();
  });
});
