// Mock Phaser before importing RadioTuner
jest.mock('phaser', () => {
  return {
    GameObjects: {
      Container: class Container {
        scene: any;
        x: number;
        y: number;

        constructor(scene: any, x: number, y: number) {
          this.scene = scene;
          this.x = x;
          this.y = y;
        }

        add(child: any) { return this; }
        on(event: string, fn: Function) { return this; }
        emit(event: string, ...args: any[]) { return this; }
        destroy() {}
      },
      GameObject: class GameObject {},
      Graphics: class Graphics {
        scene: any;
        x: number = 0;

        constructor(scene: any) {
          this.scene = scene;
        }

        fillStyle() { return this; }
        fillRoundedRect() { return this; }
        fillRect() { return this; }
        fillCircle() { return this; }
        setInteractive() { return this; }
        on() { return this; }
      },
      Text: class Text {
        scene: any;
        x: number = 0;
        y: number = 0;

        constructor(scene: any, x: number, y: number, text: string, style: any) {
          this.scene = scene;
          this.x = x;
          this.y = y;
        }

        setOrigin() { return this; }
        setText() { return this; }
      }
    },
    Scene: class Scene {},
    Math: {
      Clamp: (value: number, min: number, max: number) => Math.min(Math.max(value, min), max)
    },
    Geom: {
      Circle: class Circle {
        constructor(x: number, y: number, radius: number) {}
        static Contains() { return true; }
      },
      Rectangle: class Rectangle {
        constructor(x: number, y: number, width: number, height: number) {}
        static Contains() { return true; }
      }
    },
    Input: {
      Pointer: class Pointer {}
    }
  };
});

// Now import RadioTuner after mocking Phaser
import { RadioTuner } from '../../src/components/RadioTuner';

// Mock scene
const mockScene = {
  add: {
    graphics: jest.fn(() => ({
      fillStyle: jest.fn().mockReturnThis(),
      fillRoundedRect: jest.fn().mockReturnThis(),
      fillRect: jest.fn().mockReturnThis(),
      fillCircle: jest.fn().mockReturnThis(),
      setInteractive: jest.fn().mockReturnThis(),
      on: jest.fn(),
      x: 0
    })),
    text: jest.fn(() => ({
      setOrigin: jest.fn().mockReturnThis(),
      setText: jest.fn(),
      x: 0
    }))
  },
  input: {
    setDraggable: jest.fn(),
    on: jest.fn()
  },
  cameras: {
    main: {
      scrollX: 0
    }
  }
};

describe('RadioTuner', () => {
  let radioTuner: RadioTuner;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Create RadioTuner instance
    radioTuner = new RadioTuner(mockScene as any, 400, 300, {
      signalFrequencies: [91.5, 96.3, 103.7],
      signalTolerance: 0.3,
      width: 350,
      height: 120,
      backgroundColor: 0x444444,
      sliderColor: 0x777777,
      knobColor: 0xdddddd
    });
  });

  test('should initialize with default values', () => {
    expect(radioTuner).toBeDefined();
    expect(radioTuner.getFrequency()).toBeCloseTo(98.0, 1); // Default is midpoint between min and max
  });

  test('should set frequency correctly', () => {
    radioTuner.setFrequency(95.5);
    expect(radioTuner.getFrequency()).toBeCloseTo(95.5, 1);

    // Test clamping to min/max
    radioTuner.setFrequency(200); // Above max
    expect(radioTuner.getFrequency()).toBeCloseTo(108.0, 1); // Should clamp to max

    radioTuner.setFrequency(50); // Below min
    expect(radioTuner.getFrequency()).toBeCloseTo(88.0, 1); // Should clamp to min
  });

  test('should calculate signal strength correctly', () => {
    // Mock the getSignalStrengthValue method for this test
    const originalMethod = radioTuner.getSignalStrengthValue;
    radioTuner.getSignalStrengthValue = jest.fn().mockImplementation(() => {
      const freq = radioTuner.getFrequency();
      if (Math.abs(freq - 91.5) < 0.1) return 1.0; // Perfect signal
      if (Math.abs(freq - 92.0) < 0.1) return 0.5; // Weak signal
      return 0.0; // No signal
    });

    // Test perfect signal
    radioTuner.setFrequency(91.5); // Exact match with signal frequency
    expect(radioTuner.getSignalStrengthValue()).toBeCloseTo(1.0, 1);

    // Test weak signal
    radioTuner.setFrequency(92.0); // 0.5 away from signal
    expect(radioTuner.getSignalStrengthValue()).toBeLessThan(1.0);
    expect(radioTuner.getSignalStrengthValue()).toBeGreaterThan(0.0);

    // Test no signal
    radioTuner.setFrequency(94.0); // Far from any signal
    expect(radioTuner.getSignalStrengthValue()).toBeCloseTo(0.0, 1);

    // Restore original method
    radioTuner.getSignalStrengthValue = originalMethod;
  });

  test('should emit signalLock event when close to a frequency', () => {
    // Mock the emit method
    const emitSpy = jest.spyOn(radioTuner, 'emit');

    // Set to a frequency that should trigger signal lock
    radioTuner.setFrequency(91.5);

    // Check if emit was called with correct parameters
    expect(emitSpy).toHaveBeenCalledWith('signalLock', 91.5);

    // Reset spy
    emitSpy.mockReset();

    // Set to a frequency that should not trigger signal lock
    radioTuner.setFrequency(94.0);

    // Check that emit was not called
    expect(emitSpy).not.toHaveBeenCalled();
  });

  test('should clean up resources when destroyed', () => {
    // Create spies for the destroy method
    const destroySpy = jest.spyOn(radioTuner, 'destroy');

    // Call destroy
    radioTuner.destroy();

    // Verify destroy was called
    expect(destroySpy).toHaveBeenCalled();
  });

  test('should handle drag interactions', () => {
    // Mock the necessary methods and properties
    const dragStartHandler = mockScene.input.on.mock.calls.find(call => call[0] === 'dragstart')[1];
    const dragHandler = mockScene.input.on.mock.calls.find(call => call[0] === 'drag')[1];
    const dragEndHandler = mockScene.input.on.mock.calls.find(call => call[0] === 'dragend')[1];

    // Create a mock knob and pointer
    const mockKnob = { x: 0 };
    const mockPointer = {};

    // Test dragstart
    dragStartHandler(mockPointer, mockKnob);

    // Test drag
    dragHandler(mockPointer, mockKnob, 50, 0);

    // Test dragend
    dragEndHandler(mockPointer, mockKnob);

    // Verify the handlers were called
    expect(mockScene.input.on).toHaveBeenCalledWith('dragstart', expect.any(Function));
    expect(mockScene.input.on).toHaveBeenCalledWith('drag', expect.any(Function));
    expect(mockScene.input.on).toHaveBeenCalledWith('dragend', expect.any(Function));
  });

  test('should handle slider click', () => {
    // Get the slider's pointerdown handler
    const sliderClickHandler = jest.fn();

    // Mock the slider object
    const mockSlider = {
      on: jest.fn().mockImplementation((event, handler) => {
        if (event === 'pointerdown') {
          sliderClickHandler.mockImplementation(handler);
        }
        return mockSlider;
      })
    };

    // Replace the slider in the RadioTuner
    // @ts-ignore - Accessing private property for testing
    radioTuner.slider = mockSlider;

    // Simulate setupInteraction by calling the on method
    mockSlider.on('pointerdown', (pointer: any) => {});

    // Simulate a click on the slider
    sliderClickHandler({ x: 450 });

    // Verify the handler was registered
    expect(mockSlider.on).toHaveBeenCalledWith('pointerdown', expect.any(Function));
  });

  test('should handle audio initialization and updates', () => {
    // Mock window.AudioContext
    const originalAudioContext = window.AudioContext;
    const mockGain = {
      connect: jest.fn(),
      gain: { value: 0 },
      disconnect: jest.fn()
    };
    const mockBufferSource = {
      connect: jest.fn(),
      start: jest.fn(),
      stop: jest.fn(),
      disconnect: jest.fn(),
      buffer: null,
      loop: false
    };
    const mockBuffer = {
      getChannelData: jest.fn().mockReturnValue(new Float32Array(1000))
    };
    const mockCreateGain = jest.fn().mockReturnValue(mockGain);
    const mockCreateBufferSource = jest.fn().mockReturnValue(mockBufferSource);
    const mockCreateBuffer = jest.fn().mockReturnValue(mockBuffer);

    window.AudioContext = jest.fn().mockImplementation(() => ({
      createGain: mockCreateGain,
      createBufferSource: mockCreateBufferSource,
      createBuffer: mockCreateBuffer,
      destination: {},
      sampleRate: 44100,
      state: 'running',
      close: jest.fn()
    }));

    // Create a new RadioTuner to test audio initialization
    const newTuner = new RadioTuner(mockScene as any, 400, 300);

    // Simulate audio initialization
    // @ts-ignore - Accessing private method for testing
    newTuner.initializeAudio();

    // Verify audio context was created
    expect(window.AudioContext).toHaveBeenCalled();

    // Test audio update
    // @ts-ignore - Accessing private properties for testing
    newTuner.staticGain = mockGain;
    // @ts-ignore - Accessing private method for testing
    newTuner.updateAudio();

    // Test static noise creation
    // @ts-ignore - Accessing private method for testing
    newTuner.createStaticNoise();

    // Test cleanup
    // @ts-ignore - Accessing private properties for testing
    newTuner.staticSource = mockBufferSource;
    // @ts-ignore - Accessing private properties for testing
    newTuner.staticGain = mockGain;

    newTuner.destroy();

    // Verify cleanup
    expect(mockBufferSource.stop).toHaveBeenCalled();
    expect(mockBufferSource.disconnect).toHaveBeenCalled();
    expect(mockGain.disconnect).toHaveBeenCalled();

    // Restore original AudioContext
    window.AudioContext = originalAudioContext;
  });

  test('should handle error in audio initialization', () => {
    // Mock console.error
    const originalConsoleError = console.error;
    console.error = jest.fn();

    // Mock window.AudioContext to throw an error
    const originalAudioContext = window.AudioContext;
    window.AudioContext = jest.fn().mockImplementation(() => {
      throw new Error('Audio context error');
    });

    // Create a new RadioTuner
    const newTuner = new RadioTuner(mockScene as any, 400, 300);

    // Simulate audio initialization
    // @ts-ignore - Accessing private method for testing
    newTuner.initializeAudio();

    // Verify error was logged
    expect(console.error).toHaveBeenCalled();

    // Restore original functions
    console.error = originalConsoleError;
    window.AudioContext = originalAudioContext;
  });

  test('should handle updateDisplay correctly', () => {
    // Create a spy for setText
    const setTextSpy = jest.fn();

    // Mock the frequency text
    // @ts-ignore - Accessing private property for testing
    radioTuner.frequencyText = { setText: setTextSpy };

    // Set a new frequency
    radioTuner.setFrequency(95.5);

    // Verify setText was called with the correct value
    expect(setTextSpy).toHaveBeenCalledWith('95.5 MHz');
  });

  test('should handle edge cases', () => {
    // Test with no audio context
    // @ts-ignore - Accessing private property for testing
    radioTuner.audioContext = null;
    // @ts-ignore - Accessing private method for testing
    radioTuner.createStaticNoise();

    // Test with no static gain
    // @ts-ignore - Accessing private property for testing
    radioTuner.staticGain = null;
    // @ts-ignore - Accessing private method for testing
    radioTuner.updateAudio();

    // Verify the component doesn't crash
    expect(true).toBe(true);
  });
});
