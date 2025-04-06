import { VolumeControl } from '../../src/components/VolumeControl';
import { AudioManager } from '../../src/audio/AudioManager';

// Mock the AudioManager
jest.mock('../../src/audio/AudioManager', () => {
  return {
    AudioManager: {
      getInstance: jest.fn().mockReturnValue({
        setMasterVolume: jest.fn(),
        getMasterVolume: jest.fn().mockReturnValue(0.8), // Default to 80%
        addVolumeChangeListener: jest.fn(),
        removeVolumeChangeListener: jest.fn(),
      }),
    },
  };
});

// Mock Phaser
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

        add(_child: any) {
          return this;
        }
      },
      Graphics: class Graphics {
        scene: any;
        x: number = 0;
        y: number = 0;

        constructor(scene: any) {
          this.scene = scene;
        }

        fillStyle() {
          return this;
        }
        fillRoundedRect() {
          return this;
        }
        fillCircle() {
          return this;
        }
        setInteractive() {
          return this;
        }
        on = jest.fn().mockImplementation(function () {
          return this;
        });
      },
      Text: class Text {
        scene: any;
        x: number = 0;
        y: number = 0;
        text: string = '';

        constructor(scene: any, x: number, y: number, text: string) {
          this.scene = scene;
          this.x = x;
          this.y = y;
          this.text = text;
        }

        setOrigin() {
          return this;
        }
        setText = jest.fn().mockImplementation(function (text: string) {
          this.text = text;
          return this;
        });
      },
    },
    Geom: {
      Rectangle: class Rectangle {
        constructor() {}
      },
      Circle: class Circle {
        constructor() {}
      },
    },
    Math: {
      Clamp: (value: number, min: number, max: number) => {
        return Math.min(Math.max(value, min), max);
      },
      Easing: {
        Cubic: {
          // Actual cubic easing function for testing
          Out: (v: number) => 1 - Math.pow(1 - v, 3),
        },
      },
    },
  };
});

describe('VolumeControl', () => {
  let volumeControl: VolumeControl;
  let mockScene: any;
  let mockAudioManager: any;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Create mock scene
    mockScene = {
      add: {
        graphics: jest
          .fn()
          .mockReturnValue(new (jest.requireMock('phaser').GameObjects.Graphics)({})),
        text: jest
          .fn()
          .mockReturnValue(new (jest.requireMock('phaser').GameObjects.Text)({}, 0, 0, '')),
        existing: jest.fn(),
      },
      input: {
        setDraggable: jest.fn(),
        on: jest.fn(),
      },
      cameras: {
        main: {
          scrollX: 0,
        },
      },
    };

    // Get mock AudioManager
    mockAudioManager = AudioManager.getInstance();
  });

  test('should initialize with default values', () => {
    volumeControl = new VolumeControl(mockScene as any, 100, 100);
    expect(volumeControl).toBeDefined();

    // Check that AudioManager was called with the correct initial volume
    expect(mockAudioManager.setMasterVolume).toHaveBeenCalledWith(0.8);
  });

  test('should initialize with custom values', () => {
    volumeControl = new VolumeControl(mockScene as any, 100, 100, {
      width: 200,
      height: 40,
      backgroundColor: 0x555555,
      sliderColor: 0x777777,
      knobColor: 0xeeeeee,
      initialVolume: 0.5,
    });
    expect(volumeControl).toBeDefined();

    // Check that AudioManager was called with the custom initial volume
    expect(mockAudioManager.setMasterVolume).toHaveBeenCalledWith(0.5);
  });

  test('should apply volume curve for more natural adjustment', () => {
    volumeControl = new VolumeControl(mockScene as any, 100, 100);

    // Mock the private applyVolumeCurve method to test it directly
    const applyVolumeCurveSpy = jest.spyOn(volumeControl as any, 'applyVolumeCurve');

    // Call setVolume with a linear value
    volumeControl.setVolume(0.5);

    // Check that applyVolumeCurve was called
    expect(applyVolumeCurveSpy).toHaveBeenCalledWith(0.5);

    // Check that the curved value is different from the linear value
    // The exact value depends on the curve implementation
    const curvedValue = applyVolumeCurveSpy.mock.results[0].value;
    expect(curvedValue).not.toBe(0.5);
  });

  test('should update display with correct volume percentage', () => {
    volumeControl = new VolumeControl(mockScene as any, 100, 100);

    // Mock the volumeText object
    const mockVolumeText = new (jest.requireMock('phaser').GameObjects.Text)({}, 0, 0, '');
    (volumeControl as any).volumeText = mockVolumeText;

    // Mock the knob object
    const mockKnob = new (jest.requireMock('phaser').GameObjects.Graphics)({});
    (volumeControl as any).knob = mockKnob;

    // Set a specific volume
    mockAudioManager.getMasterVolume.mockReturnValue(0.8);

    // Call updateDisplay
    (volumeControl as any).updateDisplay();

    // Check that the text was updated correctly
    expect(mockVolumeText.setText).toHaveBeenCalledWith('Volume: 80%');
  });

  test('should handle drag events correctly', () => {
    volumeControl = new VolumeControl(mockScene as any, 100, 100);

    // Get the drag handler
    const dragHandler = mockScene.input.on.mock.calls.find((call: any) => call[0] === 'drag')[1];

    // Create mock objects
    const mockKnob = { x: 0 };
    const mockPointer = {};

    // Set isDragging to true
    (volumeControl as any).isDragging = true;
    (volumeControl as any).knob = mockKnob;

    // Call the drag handler
    dragHandler(mockPointer, mockKnob, 50);

    // Check that the volume was updated
    expect(mockAudioManager.setMasterVolume).toHaveBeenCalled();
  });

  test('should handle direct clicks on the slider', () => {
    volumeControl = new VolumeControl(mockScene as any, 100, 100);

    // Mock the background object
    const mockBackground = new (jest.requireMock('phaser').GameObjects.Graphics)({});
    (volumeControl as any).background = mockBackground;

    // Get the on method
    const onMethod = mockBackground.on;

    // Check that the on method was called with 'pointerdown'
    expect(onMethod).toHaveBeenCalledWith('pointerdown', expect.any(Function));

    // Get the pointerdown handler
    const pointerdownHandler = onMethod.mock.calls.find(
      (call: any) => call[0] === 'pointerdown'
    )[1];

    // Create a mock pointer
    const mockPointer = { x: 150 };

    // Set up volumeControl properties
    (volumeControl as any).x = 100;
    (volumeControl as any).knob = { x: 0 };

    // Call the pointerdown handler
    pointerdownHandler(mockPointer);

    // Check that the volume was updated
    expect(mockAudioManager.setMasterVolume).toHaveBeenCalled();
  });

  test('should get current volume', () => {
    volumeControl = new VolumeControl(mockScene as any, 100, 100);

    // Mock the AudioManager.getMasterVolume method
    mockAudioManager.getMasterVolume.mockReturnValue(0.8);

    // Call getVolume
    const volume = volumeControl.getVolume();

    // Check that the correct volume was returned
    expect(volume).toBe(0.8);
    expect(mockAudioManager.getMasterVolume).toHaveBeenCalled();
  });
});
