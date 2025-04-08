/**
 * Type definitions for general test mocks
 */

/**
 * Mock EventEmitter interface
 */
export interface MockEventEmitter {
  on: jest.Mock;
  off: jest.Mock;
  emit: jest.Mock;
  removeAllListeners: jest.Mock;
}

/**
 * Mock DOM Element interface
 */
export interface MockElement {
  style: Partial<CSSStyleDeclaration>;
  setAttribute: jest.Mock;
  getAttribute: jest.Mock;
  addEventListener: jest.Mock;
  dispatchEvent: jest.Mock;
  parentElement: any;
  getBoundingClientRect?: jest.Mock;
  children?: any[];
  appendChild?: jest.Mock;
  removeChild?: jest.Mock;
  classList?: {
    add: jest.Mock;
    remove: jest.Mock;
    contains: jest.Mock;
  };
  innerHTML?: string;
}

/**
 * Mock Performance Monitor interface
 */
export interface MockPerformanceMonitor {
  addEventListener: jest.Mock;
  removeEventListener: jest.Mock;
  getFPS: jest.Mock;
  getMemoryUsage: jest.Mock;
  startTracking: jest.Mock;
  stopTracking: jest.Mock;
}

/**
 * Mock SaveManager interface
 */
export interface MockSaveManager {
  setData: jest.Mock;
  getData: jest.Mock;
  hasData: jest.Mock;
  removeData: jest.Mock;
  saveToLocalStorage: jest.Mock;
  loadFromLocalStorage: jest.Mock;
}

/**
 * Mock Item interface
 */
export interface MockItem {
  id: string;
  name: string;
  description: string;
  type: string;
  icon: string;
  usable: boolean;
  stackable: boolean;
  maxStack?: number;
  effects?: any;
}

/**
 * Mock Inventory interface
 */
export interface MockInventory {
  addItem: jest.Mock;
  removeItem: jest.Mock;
  hasItem: jest.Mock;
  getItem: jest.Mock;
  getAllItems: jest.Mock;
  useItem: jest.Mock;
  on: jest.Mock;
  off: jest.Mock;
  emit: jest.Mock;
}
