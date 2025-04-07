// Mock Phaser before importing AudioVisualizer
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Mock the AudioManager
jest.mock('../../src/audio/AudioManager', () => ({
  AudioManager: {
    getInstance: jest.fn().mockReturnValue({
      setMasterVolume: jest.fn(),
      getMasterVolume: jest.fn().mockReturnValue(0.8),
      addVolumeChangeListener: jest.fn(),
      removeVolumeChangeListener: jest.fn(),
    }),
  },
}));

// Import after mocking
import { AudioVisualizer } from '../../src/components/AudioVisualizer';

describe('AudioVisualizer', () => {
  // Mock scene
  const mockScene = {
    add: {
      graphics: jest.fn().mockReturnThis(),
      existing: jest.fn(),
    },
    events: {
      on: jest.fn(),
      off: jest.fn(),
    },
    sys: {
      game: {
        loop: {
          delta: 16,
        },
      },
    },
  };

  // Mock analyzer node
  const mockAnalyser = {
    fftSize: 256,
    frequencyBinCount: 128,
    getByteFrequencyData: jest.fn(),
    getByteTimeDomainData: jest.fn(),
  };

  let audioVisualizer: AudioVisualizer;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Create AudioVisualizer instance
    audioVisualizer = new AudioVisualizer(mockScene as any, 400, 300, {
      width: 300,
      height: 100,
      backgroundColor: 0x333333,
      barColor: 0x00ff00,
      lineColor: 0xff0000,
    });

    // Mock the analyser node
    Object.defineProperty(audioVisualizer, 'analyser', {
      get: jest.fn().mockReturnValue(mockAnalyser),
      set: jest.fn(),
    });

    // Mock the dataArray
    Object.defineProperty(audioVisualizer, 'dataArray', {
      get: jest.fn().mockReturnValue(new Uint8Array(128).fill(128)),
      set: jest.fn(),
    });

    // Mock the timeDataArray
    Object.defineProperty(audioVisualizer, 'timeDataArray', {
      get: jest.fn().mockReturnValue(new Uint8Array(128).fill(128)),
      set: jest.fn(),
    });
  });

  test('should initialize with default values', () => {
    expect(audioVisualizer).toBeDefined();
    expect(mockScene.add.graphics).toHaveBeenCalled();
  });

  test('should connect to an audio node', () => {
    // Mock audio node with context
    const mockAudioContext = {
      createAnalyser: jest.fn().mockReturnValue({
        ...mockAnalyser,
        connect: jest.fn(),
        frequencyBinCount: 128,
      }),
    };

    const mockAudioNode = {
      connect: jest.fn(),
      context: mockAudioContext,
    };

    // Connect to the audio node
    audioVisualizer.connectTo(mockAudioNode as any);

    // Verify the connection was made
    expect(mockAudioContext.createAnalyser).toHaveBeenCalled();
  });

  test('should update visualization on preUpdate', () => {
    // Mock the clear and visualization methods
    const clearSpy = jest.spyOn(audioVisualizer as any, 'clear').mockImplementation(() => {});
    const drawBarsSpy = jest.spyOn(audioVisualizer as any, 'drawBars').mockImplementation(() => {});
    const drawWaveformSpy = jest
      .spyOn(audioVisualizer as any, 'drawWaveform')
      .mockImplementation(() => {});

    // Call preUpdate
    audioVisualizer.preUpdate();

    // Verify methods were called
    expect(clearSpy).toHaveBeenCalled();
    expect(drawBarsSpy).toHaveBeenCalled();
    expect(drawWaveformSpy).toHaveBeenCalled();

    // Restore spies
    clearSpy.mockRestore();
    drawBarsSpy.mockRestore();
    drawWaveformSpy.mockRestore();
  });

  test('should clear the graphics', () => {
    // Mock the graphics object
    const mockGraphics = {
      clear: jest.fn().mockReturnThis(),
      fillStyle: jest.fn().mockReturnThis(),
      fillRect: jest.fn().mockReturnThis(),
    };

    // Set the graphics object
    Object.defineProperty(audioVisualizer, 'graphics', {
      get: jest.fn().mockReturnValue(mockGraphics),
      set: jest.fn(),
    });

    // Call the clear method
    (audioVisualizer as any).clear();

    // Verify the graphics were cleared
    expect(mockGraphics.clear).toHaveBeenCalled();
    expect(mockGraphics.fillStyle).toHaveBeenCalled();
    expect(mockGraphics.fillRect).toHaveBeenCalled();
  });

  test('should draw frequency bars', () => {
    // Mock the graphics object
    const mockGraphics = {
      fillStyle: jest.fn().mockReturnThis(),
      fillRect: jest.fn().mockReturnThis(),
    };

    // Set the graphics object
    Object.defineProperty(audioVisualizer, 'graphics', {
      get: jest.fn().mockReturnValue(mockGraphics),
      set: jest.fn(),
    });

    // Call the drawBars method
    (audioVisualizer as any).drawBars();

    // Verify the bars were drawn
    expect(mockGraphics.fillStyle).toHaveBeenCalled();
    expect(mockGraphics.fillRect).toHaveBeenCalled();
  });

  test('should draw waveform', () => {
    // Mock the graphics object
    const mockGraphics = {
      lineStyle: jest.fn().mockReturnThis(),
      beginPath: jest.fn().mockReturnThis(),
      moveTo: jest.fn().mockReturnThis(),
      lineTo: jest.fn().mockReturnThis(),
      strokePath: jest.fn().mockReturnThis(),
    };

    // Set the graphics object
    Object.defineProperty(audioVisualizer, 'graphics', {
      get: jest.fn().mockReturnValue(mockGraphics),
      set: jest.fn(),
    });

    // Call the drawWaveform method
    (audioVisualizer as any).drawWaveform();

    // Verify the waveform was drawn
    expect(mockGraphics.lineStyle).toHaveBeenCalled();
    expect(mockGraphics.beginPath).toHaveBeenCalled();
    expect(mockGraphics.moveTo).toHaveBeenCalled();
    expect(mockGraphics.lineTo).toHaveBeenCalled();
    expect(mockGraphics.strokePath).toHaveBeenCalled();
  });

  test('should set visualization mode', () => {
    // Set visualization mode to bars only
    audioVisualizer.setMode('bars');
    expect((audioVisualizer as any).config.showBars).toBe(true);
    expect((audioVisualizer as any).config.showWaveform).toBe(false);

    // Set visualization mode to waveform only
    audioVisualizer.setMode('waveform');
    expect((audioVisualizer as any).config.showBars).toBe(false);
    expect((audioVisualizer as any).config.showWaveform).toBe(true);

    // Set visualization mode to both
    audioVisualizer.setMode('both');
    expect((audioVisualizer as any).config.showBars).toBe(true);
    expect((audioVisualizer as any).config.showWaveform).toBe(true);

    // Set visualization mode to none
    audioVisualizer.setMode('none');
    expect((audioVisualizer as any).config.showBars).toBe(false);
    expect((audioVisualizer as any).config.showWaveform).toBe(false);
  });

  test('should handle destroy', () => {
    // Mock the scene events off method
    const sceneEventsOffSpy = jest.spyOn(mockScene.events, 'off');

    // Call destroy
    audioVisualizer.destroy();

    // Verify the scene events off method was called
    expect(sceneEventsOffSpy).toHaveBeenCalled();

    // Restore spy
    sceneEventsOffSpy.mockRestore();
  });
});
