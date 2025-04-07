// Mock Phaser before importing SaveLoadMenu
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Mock SaveManager
jest.mock('../../src/utils/SaveManager', () => ({
  SaveManager: {
    exportState: jest.fn().mockReturnValue('{"flags":{"test_flag":true}}'),
    importState: jest.fn().mockReturnValue(true),
    getAllFlags: jest.fn().mockReturnValue({ test_flag: true }),
    getData: jest.fn(),
    setData: jest.fn(),
  },
}));

// Import after mocking
import { SaveLoadMenu } from '../../src/components/SaveLoadMenu';
import { SaveManager } from '../../src/utils/SaveManager';

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value.toString();
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    clear: jest.fn(() => {
      store = {};
    }),
  };
})();
Object.defineProperty(window, 'localStorage', { value: localStorageMock });

// Mock document methods
document.createElement = jest.fn().mockImplementation((tag) => {
  if (tag === 'input') {
    return {
      type: '',
      style: {},
      value: '',
      addEventListener: jest.fn(),
      focus: jest.fn(),
      click: jest.fn(),
    };
  }
  if (tag === 'a') {
    return {
      href: '',
      download: '',
      click: jest.fn(),
    };
  }
  return {};
});

document.body.appendChild = jest.fn();
document.body.removeChild = jest.fn();
document.body.contains = jest.fn().mockReturnValue(true);

// Mock URL methods
global.URL.createObjectURL = jest.fn().mockReturnValue('blob:test');
global.URL.revokeObjectURL = jest.fn();

// Mock Blob
global.Blob = jest.fn().mockImplementation((content, options) => ({
  content,
  options,
}));

describe('SaveLoadMenu', () => {
  let saveLoadMenu: SaveLoadMenu;
  let mockScene: any;
  let mockEmit: jest.Mock;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();
    localStorageMock.clear();

    // Create mock scene
    mockScene = {
      add: {
        rectangle: jest.fn().mockReturnValue({
          setOrigin: jest.fn().mockReturnThis(),
          setInteractive: jest.fn().mockReturnThis(),
        }),
        text: jest.fn().mockReturnValue({
          setOrigin: jest.fn().mockReturnThis(),
          setInteractive: jest.fn().mockReturnThis(),
          setBackgroundColor: jest.fn().mockReturnThis(),
          on: jest.fn().mockReturnThis(),
          destroy: jest.fn(),
        }),
        container: jest.fn().mockReturnValue({
          add: jest.fn(),
          removeAll: jest.fn(),
        }),
        existing: jest.fn(),
      },
      game: {
        canvas: {
          width: 800,
          height: 600,
          getBoundingClientRect: jest.fn().mockReturnValue({
            left: 0,
            top: 0,
          }),
        },
      },
    };

    // Mock setVisible method before creating instance
    const originalSetVisible = SaveLoadMenu.prototype.setVisible;
    SaveLoadMenu.prototype.setVisible = jest.fn();

    // Create SaveLoadMenu instance
    saveLoadMenu = new SaveLoadMenu(mockScene as any, 400, 300);

    // Restore original setVisible method
    SaveLoadMenu.prototype.setVisible = originalSetVisible;

    // Mock emit method
    mockEmit = jest.fn();
    saveLoadMenu.emit = mockEmit;
  });

  test('should create UI elements on construction', () => {
    // Check if UI elements were created
    expect(mockScene.add.rectangle).toHaveBeenCalled();
    expect(mockScene.add.text).toHaveBeenCalledWith(
      0,
      -170,
      'Save / Load Game',
      expect.any(Object)
    );
    expect(mockScene.add.text).toHaveBeenCalledWith(280, -170, 'X', expect.any(Object));
    expect(mockScene.add.container).toHaveBeenCalled();
    expect(mockScene.add.existing).toHaveBeenCalledWith(saveLoadMenu);
  });

  test('should toggle visibility', () => {
    // Mock setVisible method
    const setVisibleMock = jest.fn();
    saveLoadMenu.setVisible = setVisibleMock;

    // Show menu
    saveLoadMenu.show();
    expect(setVisibleMock).toHaveBeenCalledWith(true);
    expect(mockEmit).toHaveBeenCalledWith('show');

    // Hide menu
    saveLoadMenu.hide();
    expect(setVisibleMock).toHaveBeenCalledWith(false);
    expect(mockEmit).toHaveBeenCalledWith('hide');

    // Toggle menu (show)
    saveLoadMenu.toggle();
    expect(setVisibleMock).toHaveBeenCalledWith(true);

    // Toggle menu (hide)
    saveLoadMenu.toggle();
    expect(setVisibleMock).toHaveBeenCalledWith(false);
  });

  test('should load save slots from localStorage', () => {
    // Setup mock save slots
    const mockSaveSlots = [
      {
        id: 'save_1',
        name: 'Test Save 1',
        timestamp: Date.now(),
        data: '{"flags":{"test_flag":true}}',
      },
    ];
    localStorageMock.setItem('signal-lost-save-slots', JSON.stringify(mockSaveSlots));

    // Mock setVisible method before creating instance
    const originalSetVisible = SaveLoadMenu.prototype.setVisible;
    SaveLoadMenu.prototype.setVisible = jest.fn();

    // Create new instance to trigger loadSaveSlots
    const menu = new SaveLoadMenu(mockScene as any, 400, 300);

    // Restore original setVisible method
    SaveLoadMenu.prototype.setVisible = originalSetVisible;

    // Check if slots were loaded
    expect(localStorageMock.getItem).toHaveBeenCalledWith('signal-lost-save-slots');
    expect(mockScene.add.text).toHaveBeenCalledWith(
      0,
      -80,
      expect.stringContaining('Test Save 1'),
      expect.any(Object)
    );
  });

  test('should handle save game action', () => {
    // Mock private methods
    const createSaveSlotMock = jest.fn();
    (saveLoadMenu as any).createSaveSlot = createSaveSlotMock;

    // Call saveGame
    (saveLoadMenu as any).saveGame();

    // Check if input field was positioned
    expect(document.createElement).toHaveBeenCalledWith('input');
  });

  test('should create a save slot', () => {
    // Setup
    const saveSlotsToStorageMock = jest.fn();
    const updateSlotDisplayMock = jest.fn();
    (saveLoadMenu as any).saveSlotsToStorage = saveSlotsToStorageMock;
    (saveLoadMenu as any).updateSlotDisplay = updateSlotDisplayMock;

    // Call createSaveSlot
    (saveLoadMenu as any).createSaveSlot('Test Save');

    // Check if SaveManager.exportState was called
    expect(SaveManager.exportState).toHaveBeenCalled();

    // Check if slot was created and saved
    expect(saveSlotsToStorageMock).toHaveBeenCalled();
    expect(updateSlotDisplayMock).toHaveBeenCalled();
    expect(mockEmit).toHaveBeenCalledWith('save', expect.any(String));
  });

  test('should handle load game action', () => {
    // Setup
    (saveLoadMenu as any).saveSlots = [
      {
        id: 'save_1',
        name: 'Test Save',
        timestamp: Date.now(),
        data: '{"flags":{"test_flag":true}}',
      },
    ];
    (saveLoadMenu as any).selectedSlot = 0;

    // Call loadGame
    (saveLoadMenu as any).loadGame();

    // Check if SaveManager.importState was called
    expect(SaveManager.importState).toHaveBeenCalledWith('{"flags":{"test_flag":true}}');
    expect(mockEmit).toHaveBeenCalledWith('load', 'save_1');
  });

  test('should handle delete game action', () => {
    // Setup
    (saveLoadMenu as any).saveSlots = [
      {
        id: 'save_1',
        name: 'Test Save',
        timestamp: Date.now(),
        data: '{"flags":{"test_flag":true}}',
      },
    ];
    (saveLoadMenu as any).selectedSlot = 0;
    const saveSlotsToStorageMock = jest.fn();
    const updateSlotDisplayMock = jest.fn();
    (saveLoadMenu as any).saveSlotsToStorage = saveSlotsToStorageMock;
    (saveLoadMenu as any).updateSlotDisplay = updateSlotDisplayMock;

    // Call deleteGame
    (saveLoadMenu as any).deleteGame();

    // Check if slot was deleted
    expect(saveSlotsToStorageMock).toHaveBeenCalled();
    expect(updateSlotDisplayMock).toHaveBeenCalled();
    expect((saveLoadMenu as any).saveSlots.length).toBe(0);
    expect((saveLoadMenu as any).selectedSlot).toBe(-1);
  });

  test('should handle export game action', () => {
    // Call exportGame
    (saveLoadMenu as any).exportGame();

    // Check if SaveManager.exportState was called
    expect(SaveManager.exportState).toHaveBeenCalled();
    expect(global.URL.createObjectURL).toHaveBeenCalled();
    expect(document.createElement).toHaveBeenCalledWith('a');
    expect(document.body.appendChild).toHaveBeenCalled();
  });

  test('should handle import game action', () => {
    // Setup
    const fileInputClickMock = jest.fn();
    (saveLoadMenu as any).fileInput = {
      click: fileInputClickMock,
    };

    // Call importGame
    (saveLoadMenu as any).importGame();

    // Check if fileInput.click was called
    expect(fileInputClickMock).toHaveBeenCalled();
  });

  test('should clean up resources on destroy', () => {
    // Call destroy
    saveLoadMenu.destroy();

    // Check if HTML elements were removed
    expect(document.body.removeChild).toHaveBeenCalled();
  });
});
