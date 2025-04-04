import { NarrativeRenderer } from '../../src/narrative/NarrativeRenderer';
import { NarrativeEngine, NarrativeEvent } from '../../src/narrative/NarrativeEngine';

// Mock NarrativeEngine
jest.mock('../../src/narrative/NarrativeEngine');

// Mock Phaser
const mockScene = {
  add: {
    rectangle: jest.fn().mockReturnValue({
      setOrigin: jest.fn().mockReturnThis(),
      setInteractive: jest.fn().mockReturnThis(),
      on: jest.fn().mockReturnThis()
    }),
    text: jest.fn().mockReturnValue({
      setOrigin: jest.fn().mockReturnThis(),
      setInteractive: jest.fn().mockReturnThis(),
      on: jest.fn().mockReturnThis(),
      setText: jest.fn().mockReturnThis(),
      setColor: jest.fn().mockReturnThis(),
      destroy: jest.fn(),
      x: 0,
      y: 0,
      height: 100
    })
  },
  input: {
    keyboard: {
      addKey: jest.fn().mockReturnValue({})
    }
  }
};

describe('NarrativeRenderer', () => {
  let narrativeRenderer: NarrativeRenderer;
  let mockEngine: jest.Mocked<NarrativeEngine>;
  
  // Sample event for testing
  const sampleEvent: NarrativeEvent = {
    id: 'test_event',
    message: 'This is a test event',
    choices: [
      {
        text: 'Option 1',
        outcome: 'trigger_event_1'
      },
      {
        text: 'Option 2',
        outcome: 'set_variable=value'
      }
    ]
  };
  
  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();
    
    // Create a mock NarrativeEngine
    mockEngine = new NarrativeEngine() as jest.Mocked<NarrativeEngine>;
    mockEngine.on = jest.fn();
    mockEngine.off = jest.fn();
    mockEngine.makeChoice = jest.fn();
    mockEngine.getCurrentEvent = jest.fn().mockReturnValue(null);
    
    // Create a NarrativeRenderer
    narrativeRenderer = new NarrativeRenderer(
      mockScene as any,
      400,
      300,
      mockEngine
    );
  });
  
  describe('constructor', () => {
    test('should create UI elements', () => {
      expect(mockScene.add.rectangle).toHaveBeenCalled();
      expect(mockScene.add.text).toHaveBeenCalled();
    });
    
    test('should set up event listeners', () => {
      expect(mockEngine.on).toHaveBeenCalledWith('narrativeEvent', expect.any(Function));
      expect(mockEngine.on).toHaveBeenCalledWith('narrativeChoice', expect.any(Function));
    });
    
    test('should be hidden initially', () => {
      expect(narrativeRenderer.isVisible()).toBe(false);
    });
  });
  
  describe('showEvent', () => {
    test('should show an event', () => {
      // Mock the text object
      const mockText = mockScene.add.text();
      // @ts-ignore - Accessing private property for testing
      narrativeRenderer.messageText = mockText;
      
      // Show an event
      narrativeRenderer.showEvent(sampleEvent);
      
      // Check that the event is displayed
      expect(mockText.setText).toHaveBeenCalledWith(sampleEvent.message);
      expect(narrativeRenderer.isVisible()).toBe(true);
      expect(narrativeRenderer.getCurrentEvent()).toBe(sampleEvent);
    });
    
    test('should create choice texts', () => {
      // Mock the text object
      const mockText = mockScene.add.text();
      // @ts-ignore - Accessing private property for testing
      narrativeRenderer.messageText = mockText;
      
      // Show an event
      narrativeRenderer.showEvent(sampleEvent);
      
      // Check that choice texts are created
      expect(mockScene.add.text).toHaveBeenCalledTimes(3); // Once for message, twice for choices
      
      // Check that the choices are interactive
      const choiceTextCalls = (mockScene.add.text as jest.Mock).mock.calls.slice(1);
      for (const call of choiceTextCalls) {
        const choiceText = mockScene.add.text();
        expect(choiceText.setInteractive).toHaveBeenCalled();
        expect(choiceText.on).toHaveBeenCalledWith('pointerover', expect.any(Function));
        expect(choiceText.on).toHaveBeenCalledWith('pointerout', expect.any(Function));
        expect(choiceText.on).toHaveBeenCalledWith('pointerdown', expect.any(Function));
      }
    });
  });
  
  describe('hide', () => {
    test('should hide the renderer', () => {
      // Show an event first
      narrativeRenderer.showEvent(sampleEvent);
      
      // Hide the renderer
      narrativeRenderer.hide();
      
      expect(narrativeRenderer.isVisible()).toBe(false);
      expect(narrativeRenderer.getCurrentEvent()).toBeNull();
    });
  });
  
  describe('update', () => {
    test('should check for keyboard input when visible', () => {
      // Show an event
      narrativeRenderer.showEvent(sampleEvent);
      
      // Mock Phaser.Input.Keyboard.JustDown
      const originalJustDown = Phaser.Input.Keyboard.JustDown;
      Phaser.Input.Keyboard.JustDown = jest.fn().mockReturnValue(true);
      
      // Update
      narrativeRenderer.update(0, 16);
      
      // Check that keyboard input was checked
      expect(mockScene.input.keyboard?.addKey).toHaveBeenCalled();
      expect(Phaser.Input.Keyboard.JustDown).toHaveBeenCalled();
      expect(mockEngine.makeChoice).toHaveBeenCalled();
      
      // Restore original
      Phaser.Input.Keyboard.JustDown = originalJustDown;
    });
    
    test('should not check for keyboard input when hidden', () => {
      // Update while hidden
      narrativeRenderer.update(0, 16);
      
      // Check that keyboard input was not checked
      expect(mockScene.input.keyboard?.addKey).not.toHaveBeenCalled();
      expect(mockEngine.makeChoice).not.toHaveBeenCalled();
    });
  });
});
