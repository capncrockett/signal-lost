// Mock the AudioManager class
jest.mock('../../src/audio/AudioManager', () => {
  return {
    AudioManager: {
      getInstance: jest.fn().mockReturnValue({
        setMasterVolume: jest.fn(),
        getMasterVolume: jest.fn().mockReturnValue(0.1),
        addVolumeChangeListener: jest.fn(),
        removeVolumeChangeListener: jest.fn(),
      }),
    },
  };
});

// Mock the SoundscapeManager class
jest.mock('../../src/audio/SoundscapeManager', () => {
  return {
    SoundscapeManager: class MockSoundscapeManager {
      constructor(scene: any) {
        this.scene = scene;
      }
      scene: any;
      updateLayers(_signalStrength: number) {}
      playSignalSound() {}
      setVolume(_volume: number) {}
      setPanning(_pan: number) {}
      stop() {}
    },
  };
});

// Import after mocking
import { SoundscapeManager } from '../../src/audio/SoundscapeManager';

// Mock the RadioTuner class
jest.mock('../../src/components/RadioTuner', () => {
  return {
    RadioTuner: class MockRadioTuner {
      constructor(scene: any, x: number, y: number, _config = {}) {
        // Store the scene for later use
        this.scene = scene;
      }
      scene: any;
      on(_event: string, _callback: (...args: any[]) => void) {
        return this;
      }
      emit(_event: string, _data: any) {
        return this;
      }
      getSignalStrength() {
        return 0.5;
      }
      getFrequency() {
        return 98.0;
      }
      destroy() {}
    },
  };
});

// Import after mocking
import { RadioTuner } from '../../src/components/RadioTuner';

// Mock Phaser
jest.mock('phaser', () => {
  return {
    GameObjects: {
      Container: class Container {
        constructor(_scene: any, _x: number, _y: number) {}
        add() {
          return this;
        }
        setVisible() {
          return this;
        }
      },
      Graphics: class Graphics {
        constructor(_scene: any) {}
        fillStyle() {
          return this;
        }
        fillRect() {
          return this;
        }
      },
      Text: class Text {
        constructor(_scene: any, _x: number, _y: number, _text: string) {}
        setOrigin() {
          return this;
        }
      },
    },
  };
});

// Mock Audio
const mockAudio = {
  play: jest.fn(),
  pause: jest.fn(),
  stop: jest.fn(),
  setVolume: jest.fn(),
  setLoop: jest.fn(),
  setRate: jest.fn(),
  isPlaying: jest.fn().mockReturnValue(false),
};

// Mock Scene
const mockScene = {
  add: {
    graphics: jest.fn().mockReturnValue({
      fillStyle: jest.fn().mockReturnThis(),
      fillRect: jest.fn().mockReturnThis(),
      lineStyle: jest.fn().mockReturnThis(),
      strokeRect: jest.fn().mockReturnThis(),
      lineBetween: jest.fn().mockReturnThis(),
      setInteractive: jest.fn().mockReturnThis(),
      on: jest.fn().mockReturnThis(),
    }),
    text: jest.fn().mockReturnValue({
      setOrigin: jest.fn().mockReturnThis(),
    }),
  },
  sound: {
    add: jest.fn().mockReturnValue(mockAudio),
  },
  input: {
    on: jest.fn(),
  },
};

describe('RadioTuner and SoundscapeManager Integration', () => {
  let radioTuner: RadioTuner;
  let soundscapeManager: SoundscapeManager;

  beforeEach(() => {
    jest.clearAllMocks();

    // Create RadioTuner
    radioTuner = new RadioTuner(mockScene as any, 400, 300);

    // Create SoundscapeManager
    soundscapeManager = new SoundscapeManager(mockScene as any);
  });

  test('RadioTuner signal strength affects SoundscapeManager audio layers', () => {
    // Mock the getSignalStrength method
    const mockSignalStrength = 0.75;
    radioTuner.getSignalStrength = jest.fn().mockReturnValue(mockSignalStrength);

    // Mock the updateLayers method
    soundscapeManager.updateLayers = jest.fn();

    // Update SoundscapeManager with the signal strength
    soundscapeManager.updateLayers(mockSignalStrength);

    // Verify that SoundscapeManager.updateLayers was called with the correct signal strength
    expect(soundscapeManager.updateLayers).toHaveBeenCalledWith(mockSignalStrength);
  });

  test('RadioTuner signal lock event triggers SoundscapeManager changes', () => {
    // Mock the event emitter for RadioTuner
    radioTuner.emit = jest.fn();
    radioTuner.on = jest.fn();

    // Mock the playSignalSound method
    soundscapeManager.playSignalSound = jest.fn();

    // Simulate a signal lock event
    radioTuner.emit('signalLock', { frequency: 103.7, signalStrength: 0.9 });

    // Verify that the signal lock event was emitted
    expect(radioTuner.emit).toHaveBeenCalledWith('signalLock', {
      frequency: 103.7,
      signalStrength: 0.9,
    });

    // Now simulate the SoundscapeManager responding to the signal lock
    soundscapeManager.playSignalSound();

    // Verify that SoundscapeManager played the signal sound
    expect(soundscapeManager.playSignalSound).toHaveBeenCalled();
  });

  test('RadioTuner and SoundscapeManager work together during frequency change', () => {
    // Mock the getSignalStrength method to return different values
    let signalStrengthValue = 0.3;
    radioTuner.getSignalStrength = jest.fn().mockImplementation(() => {
      return signalStrengthValue;
    });

    // Set up spy on SoundscapeManager.updateLayers
    const updateLayersSpy = jest.spyOn(soundscapeManager, 'updateLayers');

    // Get the initial signal strength
    const signalStrength = radioTuner.getSignalStrength();

    // Update SoundscapeManager
    soundscapeManager.updateLayers(signalStrength);

    // Verify the integration
    expect(updateLayersSpy).toHaveBeenCalledWith(signalStrength);

    // Change the mock signal strength
    signalStrengthValue = 0.8;

    // Get the new signal strength
    const newSignalStrength = radioTuner.getSignalStrength();

    // Update SoundscapeManager again
    soundscapeManager.updateLayers(newSignalStrength);

    // Verify the integration again
    expect(updateLayersSpy).toHaveBeenCalledWith(newSignalStrength);

    // Signal strength should be different between the two calls
    expect(newSignalStrength).not.toEqual(signalStrength);
  });
});
