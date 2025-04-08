/**
 * Helper functions for creating general test mocks
 */

import {
  MockEventEmitter,
  MockElement,
  MockPerformanceMonitor,
  MockSaveManager,
  MockInventory,
} from '../types/mocks';

/**
 * Create a mock EventEmitter
 * @returns A mock EventEmitter
 */
export function createMockEventEmitter(): MockEventEmitter {
  return {
    on: jest.fn(),
    off: jest.fn(),
    emit: jest.fn(),
    removeAllListeners: jest.fn(),
  };
}

/**
 * Create a mock DOM Element
 * @returns A mock DOM Element
 */
export function createMockElement(): MockElement {
  return {
    style: {},
    setAttribute: jest.fn(),
    getAttribute: jest.fn(),
    addEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
    parentElement: null,
    getBoundingClientRect: jest.fn().mockReturnValue({
      x: 0,
      y: 0,
      width: 100,
      height: 100,
      top: 0,
      left: 0,
      right: 100,
      bottom: 100,
    }),
    children: [],
    appendChild: jest.fn(),
    removeChild: jest.fn(),
    classList: {
      add: jest.fn(),
      remove: jest.fn(),
      contains: jest.fn(),
    },
    innerHTML: '',
  };
}

/**
 * Create a mock Performance Monitor
 * @returns A mock Performance Monitor
 */
export function createMockPerformanceMonitor(): MockPerformanceMonitor {
  return {
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    getFPS: jest.fn().mockReturnValue(60),
    getMemoryUsage: jest
      .fn()
      .mockReturnValue({ totalJSHeapSize: 10000000, usedJSHeapSize: 5000000 }),
    startTracking: jest.fn(),
    stopTracking: jest.fn(),
  };
}

/**
 * Create a mock SaveManager
 * @returns A mock SaveManager
 */
export function createMockSaveManager(): MockSaveManager {
  const dataStore: Record<string, any> = {};

  return {
    setData: jest.fn().mockImplementation((key: string, value: any) => {
      dataStore[key] = value;
      return true;
    }),
    getData: jest.fn().mockImplementation((key: string) => dataStore[key]),
    hasData: jest.fn().mockImplementation((key: string) => key in dataStore),
    removeData: jest.fn().mockImplementation((key: string) => {
      delete dataStore[key];
      return true;
    }),
    saveToLocalStorage: jest.fn().mockReturnValue(true),
    loadFromLocalStorage: jest.fn().mockReturnValue(true),
  };
}

/**
 * Create a mock Inventory
 * @returns A mock Inventory
 */
export function createMockInventory(): MockInventory {
  const items: Record<string, any> = {};

  return {
    addItem: jest.fn().mockImplementation((item: any) => {
      items[item.id] = item;
      return true;
    }),
    removeItem: jest.fn().mockImplementation((itemId: string) => {
      delete items[itemId];
      return true;
    }),
    hasItem: jest.fn().mockImplementation((itemId: string) => itemId in items),
    getItem: jest.fn().mockImplementation((itemId: string) => items[itemId]),
    getAllItems: jest.fn().mockReturnValue(Object.values(items)),
    useItem: jest.fn(),
    on: jest.fn(),
    off: jest.fn(),
    emit: jest.fn(),
  };
}
