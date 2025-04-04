// Mock Phaser before importing MainScene
jest.mock('phaser', () => {
  return {
    Scene: class Scene {
      constructor(config: any) {}
      add = {
        existing: jest.fn()
      };
      input = {
        once: jest.fn(),
        on: jest.fn()
      };
    },
    GameObjects: {
      Container: class Container {
        on() { return this; }
        emit() { return this; }
      }
    }
  };
});

// Import the MainScene after mocking Phaser
import { MainScene } from '../../src/scenes/MainScene';
import { RadioTuner } from '../../src/components/RadioTuner';
import { SoundscapeManager } from '../../src/audio/SoundscapeManager';

// Mock the RadioTuner component
jest.mock('../../src/components/RadioTuner');

// Mock the SoundscapeManager
jest.mock('../../src/audio/SoundscapeManager');

describe('MainScene', () => {
  let mainScene: MainScene;

  beforeEach(() => {
    jest.clearAllMocks();
    mainScene = new MainScene();
  });

  test('should initialize correctly', () => {
    expect(mainScene).toBeDefined();
    expect(mainScene.constructor.name).toBe('MainScene');
  });

  test('should create RadioTuner and SoundscapeManager in create method', () => {
    // Mock the RadioTuner constructor
    (RadioTuner as jest.Mock).mockImplementation(() => ({
      on: jest.fn().mockReturnThis()
    }));

    // Mock the SoundscapeManager constructor
    (SoundscapeManager as jest.Mock).mockImplementation(() => ({
      initialize: jest.fn().mockReturnValue(true),
      adjustLayers: jest.fn()
    }));

    // Call the create method
    mainScene.create();

    // Verify RadioTuner and SoundscapeManager were created
    expect(RadioTuner).toHaveBeenCalled();
    expect(SoundscapeManager).toHaveBeenCalled();
  });

  test('should handle signal lock events', () => {
    // Mock console.log
    const originalConsoleLog = console.log;
    console.log = jest.fn();

    // Mock the RadioTuner
    const mockRadioTuner = {
      on: jest.fn().mockImplementation((event, callback) => {
        if (event === 'signalLock') {
          // Simulate the callback being called
          callback(96.3);
        }
        return mockRadioTuner;
      })
    };
    (RadioTuner as jest.Mock).mockImplementation(() => mockRadioTuner);

    // Call the create method
    mainScene.create();

    // Verify the event handler was registered
    expect(mockRadioTuner.on).toHaveBeenCalledWith('signalLock', expect.any(Function));

    // Verify the console.log was called with the expected message
    expect(console.log).toHaveBeenCalledWith('Signal locked at frequency: 96.3');

    // Restore console.log
    console.log = originalConsoleLog;
  });

  test('should update soundscape in update method', () => {
    // Mock the RadioTuner
    const mockGetSignalStrength = jest.fn().mockReturnValue(0.5);
    (RadioTuner as jest.Mock).mockImplementation(() => ({
      on: jest.fn().mockReturnThis(),
      getSignalStrengthValue: mockGetSignalStrength
    }));

    // Mock the SoundscapeManager
    const mockAdjustLayers = jest.fn();
    (SoundscapeManager as jest.Mock).mockImplementation(() => ({
      initialize: jest.fn().mockReturnValue(true),
      adjustLayers: mockAdjustLayers
    }));

    // Create the scene and call update
    mainScene.create();
    mainScene.update(1000, 16);

    // Verify the soundscape was updated based on signal strength
    expect(mockGetSignalStrength).toHaveBeenCalled();
    expect(mockAdjustLayers).toHaveBeenCalledWith(0.5);
  });
});
