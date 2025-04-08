import type { Config } from 'jest';

const config: Config = {
  preset: 'ts-jest',
  testEnvironment: 'jsdom',
  testMatch: ['**/tests/**/*.test.ts', '**/tests/**/*.test.tsx'],
  collectCoverageFrom: [
    'src/**/*.ts',
    'src/**/*.tsx',
    '!src/main.ts',
    '!src/main.tsx',
    '!src/vite-env.d.ts',
  ],
  coverageThreshold: {
    global: {
      branches: 80,
      functions: 80,
      lines: 80,
      statements: 80,
    },
  },
  setupFilesAfterEnv: ['./tests/setup.ts'],
  // Handle ESM modules like tone.js
  transformIgnorePatterns: ['/node_modules/(?!tone)'],
  // Mock modules that cause issues
  moduleNameMapper: {
    '^tone$': '<rootDir>/tests/mocks/ToneMock.ts',
    '\\.(css|less|scss|sass)$': '<rootDir>/tests/mocks/styleMock.ts',
    '^@/(.*)$': '<rootDir>/src/$1',
  },
};

export default config;
