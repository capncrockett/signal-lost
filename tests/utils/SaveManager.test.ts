import { SaveManager } from '../../src/utils/SaveManager';

describe('SaveManager', () => {
  // Mock localStorage
  const localStorageMock = (() => {
    let store: Record<string, string> = {};

    return {
      getItem: jest.fn((key: string) => {
        return store[key] || null;
      }),
      setItem: jest.fn((key: string, value: string) => {
        store[key] = value;
      }),
      removeItem: jest.fn((key: string) => {
        delete store[key];
      }),
      clear: jest.fn(() => {
        store = {};
      }),
      // For testing
      _getStore: () => store,
    };
  })();

  // Mock console methods
  const originalConsoleWarn = console.warn;
  const originalConsoleError = console.error;
  const mockConsoleWarn = jest.fn();
  const mockConsoleError = jest.fn();

  beforeEach(() => {
    // Setup localStorage mock
    Object.defineProperty(window, 'localStorage', { value: localStorageMock });

    // Clear localStorage mock
    localStorageMock.clear();

    // Reset mocks
    jest.clearAllMocks();

    // Mock console methods
    console.warn = mockConsoleWarn;
    console.error = mockConsoleError;

    // Reset SaveManager state
    // @ts-expect-error - Accessing private property for testing
    SaveManager.flagCache = {};
    // @ts-expect-error - Accessing private property for testing
    SaveManager.isCacheInitialized = false;
  });

  afterAll(() => {
    // Restore console methods
    console.warn = originalConsoleWarn;
    console.error = originalConsoleError;
  });

  describe('initialize', () => {
    test('should initialize with empty cache when no saved state', () => {
      const result = SaveManager.initialize();

      expect(result).toBe(true);
      expect(localStorageMock.getItem).toHaveBeenCalledWith('signal-lost-game-state');

      // Check internal state
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache).toEqual({});
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.isCacheInitialized).toBe(true);
    });

    test('should load saved state from localStorage', () => {
      // Setup saved state
      const savedState = { flag1: true, flag2: false };
      localStorageMock.setItem('signal-lost-game-state', JSON.stringify(savedState));

      const result = SaveManager.initialize();

      expect(result).toBe(true);
      expect(localStorageMock.getItem).toHaveBeenCalledWith('signal-lost-game-state');

      // Check internal state
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache).toEqual(savedState);
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.isCacheInitialized).toBe(true);
    });

    test('should handle invalid JSON in localStorage', () => {
      // Setup invalid saved state
      localStorageMock.setItem('signal-lost-game-state', 'invalid-json');

      const result = SaveManager.initialize();

      expect(result).toBe(true);
      expect(localStorageMock.getItem).toHaveBeenCalledWith('signal-lost-game-state');
      expect(mockConsoleError).toHaveBeenCalled();

      // Check internal state
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache).toEqual({});
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.isCacheInitialized).toBe(true);
    });

    test('should handle localStorage access errors', () => {
      // Mock localStorage.getItem to throw an error
      localStorageMock.getItem.mockImplementationOnce(() => {
        throw new Error('localStorage access error');
      });

      const result = SaveManager.initialize();

      expect(result).toBe(false);
      expect(localStorageMock.getItem).toHaveBeenCalledWith('signal-lost-game-state');
      expect(mockConsoleError).toHaveBeenCalled();

      // Check internal state
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache).toEqual({});
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.isCacheInitialized).toBe(false);
    });
  });

  describe('getFlag', () => {
    test('should return default value for non-existent flag', () => {
      const result = SaveManager.getFlag('nonExistentFlag');

      expect(result).toBe(false); // Default is false
      expect(localStorageMock.getItem).toHaveBeenCalledWith('signal-lost-game-state');
    });

    test('should return custom default value for non-existent flag', () => {
      const result = SaveManager.getFlag('nonExistentFlag', true);

      expect(result).toBe(true);
    });

    test('should return flag value for existing flag', () => {
      // Setup flag
      SaveManager.setFlag('existingFlag', true);

      const result = SaveManager.getFlag('existingFlag');

      expect(result).toBe(true);
    });

    test('should handle empty flag id', () => {
      const result = SaveManager.getFlag('');

      expect(result).toBe(false);
      expect(mockConsoleWarn).toHaveBeenCalled();
    });
  });

  describe('setFlag', () => {
    test('should set flag value and save to localStorage', () => {
      const result = SaveManager.setFlag('testFlag', true);

      expect(result).toBe(true);
      expect(localStorageMock.setItem).toHaveBeenCalled();

      // Check flag was set
      expect(SaveManager.getFlag('testFlag')).toBe(true);
    });

    test('should handle empty flag id', () => {
      const result = SaveManager.setFlag('', true);

      expect(result).toBe(false);
      expect(mockConsoleWarn).toHaveBeenCalled();
      expect(localStorageMock.setItem).not.toHaveBeenCalled();
    });

    test('should handle localStorage save errors', () => {
      // Mock localStorage.setItem to throw an error
      localStorageMock.setItem.mockImplementationOnce(() => {
        throw new Error('localStorage save error');
      });

      const result = SaveManager.setFlag('testFlag', true);

      expect(result).toBe(false);
      expect(localStorageMock.setItem).toHaveBeenCalled();
      expect(mockConsoleError).toHaveBeenCalled();

      // Check flag was still set in memory
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache.testFlag).toBe(true);
    });
  });

  describe('clearFlags', () => {
    test('should clear all flags and save to localStorage', () => {
      // Setup flags
      SaveManager.setFlag('flag1', true);
      SaveManager.setFlag('flag2', false);

      const result = SaveManager.clearFlags();

      expect(result).toBe(true);
      expect(localStorageMock.setItem).toHaveBeenCalled();

      // Check flags were cleared
      expect(SaveManager.getFlag('flag1')).toBe(false);
      expect(SaveManager.getFlag('flag2')).toBe(false);
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache).toEqual({});
    });

    test('should handle localStorage save errors', () => {
      // Setup flags
      SaveManager.setFlag('flag1', true);

      // Mock localStorage.setItem to throw an error
      localStorageMock.setItem.mockImplementationOnce(() => {
        throw new Error('localStorage save error');
      });

      const result = SaveManager.clearFlags();

      expect(result).toBe(false);
      expect(localStorageMock.setItem).toHaveBeenCalled();
      expect(mockConsoleError).toHaveBeenCalled();

      // Check flags were still cleared in memory
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache).toEqual({});
    });
  });

  describe('hasFlag', () => {
    test('should return true for existing flag', () => {
      // Setup flag
      SaveManager.setFlag('existingFlag', true);

      const result = SaveManager.hasFlag('existingFlag');

      expect(result).toBe(true);
    });

    test('should return false for non-existent flag', () => {
      const result = SaveManager.hasFlag('nonExistentFlag');

      expect(result).toBe(false);
    });

    test('should handle empty flag id', () => {
      const result = SaveManager.hasFlag('');

      expect(result).toBe(false);
    });
  });

  describe('getAllFlags', () => {
    test('should return copy of all flags', () => {
      // Setup flags
      SaveManager.setFlag('flag1', true);
      SaveManager.setFlag('flag2', false);

      const result = SaveManager.getAllFlags();

      expect(result).toEqual({ flag1: true, flag2: false });

      // Verify it's a copy by modifying the result
      result.flag1 = false;

      // Original should not be modified
      expect(SaveManager.getFlag('flag1')).toBe(true);
    });

    test('should return empty object when no flags', () => {
      const result = SaveManager.getAllFlags();

      expect(result).toEqual({});
    });
  });

  describe('removeFlag', () => {
    test('should remove existing flag and save to localStorage', () => {
      // Setup flag
      SaveManager.setFlag('existingFlag', true);

      const result = SaveManager.removeFlag('existingFlag');

      expect(result).toBe(true);
      expect(localStorageMock.setItem).toHaveBeenCalled();

      // Check flag was removed
      expect(SaveManager.hasFlag('existingFlag')).toBe(false);
    });

    test('should handle non-existent flag', () => {
      const result = SaveManager.removeFlag('nonExistentFlag');

      expect(result).toBe(true); // Success because flag doesn't exist
      expect(localStorageMock.setItem).toHaveBeenCalled();
    });

    test('should handle empty flag id', () => {
      const result = SaveManager.removeFlag('');

      expect(result).toBe(false);
      expect(mockConsoleWarn).toHaveBeenCalled();
      expect(localStorageMock.setItem).not.toHaveBeenCalled();
    });

    test('should handle localStorage save errors', () => {
      // Setup flag
      SaveManager.setFlag('existingFlag', true);

      // Mock localStorage.setItem to throw an error
      localStorageMock.setItem.mockImplementationOnce(() => {
        throw new Error('localStorage save error');
      });

      const result = SaveManager.removeFlag('existingFlag');

      expect(result).toBe(false);
      expect(localStorageMock.setItem).toHaveBeenCalled();
      expect(mockConsoleError).toHaveBeenCalled();

      // Check flag was still removed in memory
      // @ts-expect-error - Accessing private property for testing
      expect('existingFlag' in SaveManager.flagCache).toBe(false);
    });
  });

  describe('importState', () => {
    test('should import valid state and save to localStorage', () => {
      const state = { flag1: true, flag2: false };
      const jsonState = JSON.stringify(state);

      const result = SaveManager.importState(jsonState);

      expect(result).toBe(true);
      expect(localStorageMock.setItem).toHaveBeenCalled();

      // Check state was imported
      expect(SaveManager.getFlag('flag1')).toBe(true);
      expect(SaveManager.getFlag('flag2')).toBe(false);
    });

    test('should filter out non-boolean values', () => {
      const state = { flag1: true, invalidFlag: 'not-a-boolean' };
      const jsonState = JSON.stringify(state);

      const result = SaveManager.importState(jsonState);

      expect(result).toBe(true);

      // Check only valid flags were imported
      expect(SaveManager.getFlag('flag1')).toBe(true);
      expect(SaveManager.hasFlag('invalidFlag')).toBe(false);
    });

    test('should handle invalid JSON', () => {
      const result = SaveManager.importState('invalid-json');

      expect(result).toBe(false);
      expect(mockConsoleError).toHaveBeenCalled();
      expect(localStorageMock.setItem).not.toHaveBeenCalled();
    });

    test('should handle non-object state', () => {
      const result = SaveManager.importState('"not-an-object"');

      expect(result).toBe(false);
      expect(mockConsoleError).toHaveBeenCalled();
      expect(localStorageMock.setItem).not.toHaveBeenCalled();
    });

    test('should handle empty state', () => {
      const result = SaveManager.importState('');

      expect(result).toBe(false);
      expect(mockConsoleWarn).toHaveBeenCalled();
      expect(localStorageMock.setItem).not.toHaveBeenCalled();
    });

    test('should handle localStorage save errors', () => {
      // Mock localStorage.setItem to throw an error
      localStorageMock.setItem.mockImplementationOnce(() => {
        throw new Error('localStorage save error');
      });

      const state = { flag1: true };
      const jsonState = JSON.stringify(state);

      const result = SaveManager.importState(jsonState);

      expect(result).toBe(false);
      expect(localStorageMock.setItem).toHaveBeenCalled();
      expect(mockConsoleError).toHaveBeenCalled();

      // Check state was still imported in memory
      // @ts-expect-error - Accessing private property for testing
      expect(SaveManager.flagCache).toEqual(state);
    });
  });

  describe('exportState', () => {
    test('should export state as JSON string', () => {
      // Setup flags
      SaveManager.setFlag('flag1', true);
      SaveManager.setFlag('flag2', false);

      const result = SaveManager.exportState();

      expect(JSON.parse(result)).toEqual({ flag1: true, flag2: false });
    });

    test('should export empty object when no flags', () => {
      const result = SaveManager.exportState();

      expect(JSON.parse(result)).toEqual({});
    });
  });
});
