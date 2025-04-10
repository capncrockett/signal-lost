import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import GameStateIntegration from '../../../src/components/system/GameStateIntegration';
import { CombinedGameProvider } from '../../../src/context/CombinedGameProvider';

// Mock fetch for loading triggers
global.fetch = jest.fn().mockImplementation(() =>
  Promise.resolve({
    json: () => Promise.resolve([]),
  })
) as jest.Mock;

// Mock the context hooks
jest.mock('../../../src/context/GameStateContext', () => ({
  useGameState: jest.fn().mockReturnValue({
    state: {
      currentFrequency: 90.0,
      discoveredFrequencies: [],
      inventory: [],
      gameProgress: 0,
      isRadioOn: false,
    },
    dispatch: jest.fn(),
  }),
}));

jest.mock('../../../src/context/SignalStateContext', () => ({
  useSignalState: jest.fn().mockReturnValue({
    state: {
      signals: {},
      activeSignalId: null,
      discoveredSignalIds: [],
      lastDiscoveredTimestamp: null,
    },
    getDiscoveredSignals: jest.fn().mockReturnValue([]),
    getActiveSignal: jest.fn().mockReturnValue(undefined),
    getSignalById: jest.fn(),
    updateSignal: jest.fn(),
    discoverSignal: jest.fn(),
  }),
}));

jest.mock('../../../src/context/EventContext', () => ({
  useEvent: jest.fn().mockReturnValue({
    state: {
      events: {},
      eventHistory: [],
      activeEventId: null,
      pendingEvents: [],
    },
    dispatchEvent: jest.fn(),
    getPendingEvents: jest.fn().mockReturnValue([]),
    getEventById: jest.fn(),
    markEventProcessed: jest.fn(),
  }),
}));

jest.mock('../../../src/context/ProgressContext', () => ({
  useProgress: jest.fn().mockReturnValue({
    state: {
      currentProgress: 0,
      objectives: {},
      completedObjectiveIds: [],
      lastCompletedObjectiveId: null,
      lastCompletedTimestamp: null,
    },
    setProgress: jest.fn(),
    getCompletedObjectives: jest.fn().mockReturnValue([]),
    getPendingObjectives: jest.fn().mockReturnValue([]),
    completeObjective: jest.fn(),
  }),
}));

// Mock the SaveManager
jest.mock('../../../src/utils/SaveManager', () => ({
  SaveManager: jest.fn().mockImplementation(() => ({
    startAutoSave: jest.fn(),
    stopAutoSave: jest.fn(),
    saveGame: jest.fn().mockReturnValue('test-save-id'),
  })),
}));

describe('GameStateIntegration', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders without crashing', async () => {
    render(
      <CombinedGameProvider persistState={false} enableGameStateIntegration={false}>
        <GameStateIntegration autoSaveInterval={0} />
        <div data-testid="test-content">Test Content</div>
      </CombinedGameProvider>
    );

    // The component doesn't render anything visible, so we check if the test content is rendered
    expect(screen.getByTestId('test-content')).toBeInTheDocument();
  });

  test('loads triggers from provided URLs', async () => {
    const mockFetch = global.fetch as jest.Mock;
    
    render(
      <CombinedGameProvider persistState={false} enableGameStateIntegration={false}>
        <GameStateIntegration 
          triggerUrls={['/test-triggers.json']} 
          autoSaveInterval={0} 
        />
      </CombinedGameProvider>
    );

    // Wait for the fetch to be called
    await waitFor(() => {
      expect(mockFetch).toHaveBeenCalledWith('/test-triggers.json');
    });
  });

  test('handles signal discovery', async () => {
    const mockSignalState = require('../../../src/context/SignalStateContext').useSignalState;
    const mockEventState = require('../../../src/context/EventContext').useEvent;
    
    // Mock a discovered signal
    const mockSignal = {
      id: 'test-signal',
      frequency: 91.5,
      strength: 0.8,
      type: 'message',
      content: 'test-message',
      discovered: true,
      timestamp: Date.now(),
    };
    
    mockSignalState.mockReturnValue({
      ...mockSignalState(),
      state: {
        ...mockSignalState().state,
        discoveredSignalIds: ['test-signal'],
        lastDiscoveredTimestamp: Date.now(),
      },
      getSignalById: jest.fn().mockReturnValue(mockSignal),
    });
    
    const mockDispatchEvent = jest.fn();
    mockEventState.mockReturnValue({
      ...mockEventState(),
      dispatchEvent: mockDispatchEvent,
    });
    
    const onSignalDiscovered = jest.fn();
    
    render(
      <CombinedGameProvider persistState={false} enableGameStateIntegration={false}>
        <GameStateIntegration 
          onSignalDiscovered={onSignalDiscovered}
          autoSaveInterval={0} 
        />
      </CombinedGameProvider>
    );
    
    // Wait for the signal discovery to be processed
    await waitFor(() => {
      expect(onSignalDiscovered).toHaveBeenCalledWith(mockSignal);
    });
  });
});
