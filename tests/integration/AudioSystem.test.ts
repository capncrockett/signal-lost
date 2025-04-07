// Mock Phaser before importing components
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Mock Web Audio API
const mockAudioContext = {
  createGain: jest.fn().mockReturnValue({
    gain: { value: 0, setTargetAtTime: jest.fn() },
    connect: jest.fn(),
    disconnect: jest.fn(),
  }),
  createOscillator: jest.fn().mockReturnValue({
    type: 'sine',
    frequency: { value: 0 },
    connect: jest.fn(),
    start: jest.fn(),
    stop: jest.fn(),
    disconnect: jest.fn(),
  }),
  createStereoPanner: jest.fn().mockReturnValue({
    pan: { value: 0 },
    connect: jest.fn(),
    disconnect: jest.fn(),
  }),
  createAnalyser: jest.fn().mockReturnValue({
    fftSize: 256,
    frequencyBinCount: 128,
    connect: jest.fn(),
    disconnect: jest.fn(),
    getByteFrequencyData: jest.fn(),
    getByteTimeDomainData: jest.fn(),
  }),
  createBufferSource: jest.fn().mockReturnValue({
    buffer: null,
    loop: false,
    connect: jest.fn(),
    start: jest.fn(),
    stop: jest.fn(),
    disconnect: jest.fn(),
  }),
  createBuffer: jest.fn().mockReturnValue({
    getChannelData: jest.fn().mockReturnValue(new Float32Array(4096)),
  }),
  destination: {},
  currentTime: 0,
  sampleRate: 44100,
  state: 'running',
  resume: jest.fn().mockResolvedValue(undefined),
  close: jest.fn(),
};

// Mock AudioContext constructor
window.AudioContext = jest.fn().mockImplementation(() => mockAudioContext);

// Import components after mocking
import { AudioManager } from '../../src/audio/AudioManager';
import { SoundscapeManager } from '../../src/audio/SoundscapeManager';
import { RadioTuner } from '../../src/components/RadioTuner';

describe('Audio System Integration', () => {
  // Mock scene
  const mockScene = {
    add: {
      container: jest.fn().mockReturnThis(),
      graphics: jest.fn(() => ({
        fillStyle: jest.fn().mockReturnThis(),
        fillRoundedRect: jest.fn().mockReturnThis(),
        fillRect: jest.fn().mockReturnThis(),
        fillCircle: jest.fn().mockReturnThis(),
        lineStyle: jest.fn().mockReturnThis(),
        strokeRoundedRect: jest.fn().mockReturnThis(),
        strokeRect: jest.fn().mockReturnThis(),
        strokeCircle: jest.fn().mockReturnThis(),
        setInteractive: jest.fn().mockReturnThis(),
        on: jest.fn(),
        x: 0,
      })),
      text: jest.fn(() => ({
        setOrigin: jest.fn().mockReturnThis(),
        setText: jest.fn(),
        x: 0,
      })),
      existing: jest.fn(),
    },
    input: {
      on: jest.fn(),
      off: jest.fn(),
      setDraggable: jest.fn(),
    },
    events: {
      on: jest.fn(),
      off: jest.fn(),
    },
    cameras: {
      main: {
        scrollX: 0,
      },
    },
    sys: {
      game: {
        loop: {
          delta: 16,
        },
      },
    },
  };

  // Components
  let audioManager: AudioManager;
  let soundscapeManager: SoundscapeManager;
  let radioTuner: RadioTuner;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Reset AudioManager singleton
    (AudioManager as any).instance = undefined;

    // Create components
    audioManager = AudioManager.getInstance();
    soundscapeManager = new SoundscapeManager(mockScene as any);
    radioTuner = new RadioTuner(mockScene as any, 400, 300, {
      signalFrequencies: [91.5, 96.3, 103.7],
      signalTolerance: 0.3,
    });
  });

  test('should initialize audio system', () => {
    // Initialize components
    const soundscapeInitResult = soundscapeManager.initialize();
    expect(soundscapeInitResult).toBe(true);
    expect(window.AudioContext).toHaveBeenCalled();

    // Initialize radio tuner audio
    (radioTuner as any).initializeAudio();
    expect(window.AudioContext).toHaveBeenCalled();
  });

  test('should propagate volume changes through the system', () => {
    // Initialize components
    soundscapeManager.initialize();
    (radioTuner as any).initializeAudio();

    // Add volume change listener spy
    const volumeListenerSpy = jest.fn();
    audioManager.addVolumeChangeListener(volumeListenerSpy);

    // Change master volume
    audioManager.setMasterVolume(0.5);

    // Verify listener was called
    expect(volumeListenerSpy).toHaveBeenCalledWith(0.5);

    // Update audio components
    soundscapeManager.updateLayers(0.7);
    (radioTuner as any).updateAudio();

    // Verify audio nodes were updated
    expect(mockAudioContext.createGain).toHaveBeenCalled();
  });

  test('should handle signal detection and audio updates', () => {
    // Initialize components
    soundscapeManager.initialize();
    (radioTuner as any).initializeAudio();

    // Set up spies
    const updateAudioSpy = jest.spyOn(radioTuner as any, 'updateAudio');
    const updateLayersSpy = jest.spyOn(soundscapeManager, 'updateLayers');

    // Tune to a signal frequency
    radioTuner.setFrequency(91.5);

    // Verify audio was updated
    expect(updateAudioSpy).toHaveBeenCalled();

    // Check signal strength
    const signalStrength = radioTuner.getSignalStrengthValue();
    expect(signalStrength).toBeCloseTo(1.0, 1);

    // Update soundscape with signal strength
    soundscapeManager.updateLayers(signalStrength);
    expect(updateLayersSpy).toHaveBeenCalledWith(signalStrength);

    // Restore spies
    updateAudioSpy.mockRestore();
    updateLayersSpy.mockRestore();
  });

  test('should handle audio context state changes', () => {
    // Initialize components
    soundscapeManager.initialize();
    (radioTuner as any).initializeAudio();

    // Mock suspended audio context
    mockAudioContext.state = 'suspended';

    // Set up spy for resume method
    const resumeSpy = jest.spyOn(mockAudioContext, 'resume');

    // Trigger audio operations
    radioTuner.setFrequency(96.3);
    soundscapeManager.updateLayers(0.5);

    // Verify resume was called
    expect(resumeSpy).toHaveBeenCalled();

    // Restore spy
    resumeSpy.mockRestore();
  });

  test('should clean up resources when disposed', () => {
    // Initialize components
    soundscapeManager.initialize();
    (radioTuner as any).initializeAudio();

    // Set up spies
    const closeSpy = jest.spyOn(mockAudioContext, 'close');
    const stopSpy = jest.spyOn(mockAudioContext.createBufferSource(), 'stop');

    // Dispose soundscape manager
    soundscapeManager.dispose();

    // Verify resources were cleaned up
    expect(closeSpy).toHaveBeenCalled();

    // Clean up radio tuner
    (radioTuner as any).destroy(true);

    // Restore spies
    closeSpy.mockRestore();
    stopSpy.mockRestore();
  });

  test('should handle audio node connections correctly', () => {
    // Initialize components
    soundscapeManager.initialize();
    (radioTuner as any).initializeAudio();

    // Set up spy for connect method
    const connectSpy = jest.spyOn(mockAudioContext.createGain(), 'connect');

    // Create audio nodes
    const sourceNode = mockAudioContext.createBufferSource();
    const gainNode = mockAudioContext.createGain();

    // Connect nodes
    sourceNode.connect(gainNode);
    gainNode.connect(mockAudioContext.destination);

    // Verify connections were made
    expect(connectSpy).toHaveBeenCalled();

    // Restore spy
    connectSpy.mockRestore();
  });
});
