// Mock Phaser before importing MainScene
jest.mock('phaser', () => {
  return {
    Scene: class Scene {
      constructor(config: any) {}
      add = {
        existing: jest.fn()
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

// Mock the RadioTuner component
jest.mock('../../src/components/RadioTuner');

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

  test('should create RadioTuner in create method', () => {
    // Mock the RadioTuner constructor
    (RadioTuner as jest.Mock).mockImplementation(() => ({
      on: jest.fn().mockReturnThis()
    }));

    // Call the create method
    mainScene.create();

    // Verify RadioTuner was created
    expect(RadioTuner).toHaveBeenCalled();
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

  test('should have update method', () => {
    // Call the update method
    mainScene.update(1000, 16);

    // Just verify it doesn't throw an error
    expect(true).toBe(true);
  });
});
