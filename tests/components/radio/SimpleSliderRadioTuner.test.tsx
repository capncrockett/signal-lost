import React from 'react';
import { render, screen, fireEvent, cleanup } from '@testing-library/react';
import '@testing-library/jest-dom';
import SimpleSliderRadioTuner from '../../../src/components/radio/SimpleSliderRadioTuner';

// Mock the dependencies
jest.mock('../../../src/context/AudioContext', () => ({
  useAudio: () => ({
    isMuted: false,
    volume: 0.5,
    toggleMute: jest.fn(),
    setVolume: jest.fn(),
    playStaticNoise: jest.fn(),
    stopStaticNoise: jest.fn(),
    playSignal: jest.fn(),
    stopSignal: jest.fn(),
    setNoiseType: jest.fn(),
    currentNoiseType: 'pink',
  }),
}));

// Mock the GameState context
const mockDispatch = jest.fn();
const mockGameState = {
  currentFrequency: 90.0,
  discoveredFrequencies: [],
  isRadioOn: false,
};

jest.mock('../../../src/context/GameStateContext', () => ({
  useGameState: () => ({
    state: mockGameState,
    dispatch: mockDispatch,
  }),
}));

// Mock the frequency functions
jest.mock('../../../src/data/frequencies', () => ({
  findSignalAtFrequency: jest.fn().mockImplementation((frequency) => {
    if (frequency === 91.1) {
      return {
        frequency: 91.1,
        tolerance: 0.1,
        signalStrength: 0.8,
        messageId: 'test_signal',
        isStatic: false,
      };
    }
    return null;
  }),
  calculateSignalStrength: jest.fn().mockReturnValue(0.8),
  getStaticIntensity: jest.fn().mockReturnValue(0.5),
}));

// Mock the message function
jest.mock('../../../src/data/messages', () => ({
  getMessage: jest.fn().mockReturnValue({
    id: 'test_signal',
    title: 'Test Signal',
    content: 'This is a test signal',
    isDecoded: true,
  }),
}));

describe('SimpleSliderRadioTuner Component', () => {
  // Clean up after each test
  afterEach(() => {
    cleanup();
    jest.clearAllMocks();
  });

  test('renders with default frequency', () => {
    render(<SimpleSliderRadioTuner />);
    expect(screen.getByText('90.0')).toBeInTheDocument();
    expect(screen.getByText('MHz')).toBeInTheDocument();
  });

  test('toggles radio power', () => {
    render(<SimpleSliderRadioTuner />);

    // Radio should be off initially
    expect(screen.getByText('OFF')).toBeInTheDocument();

    // Click the power button
    fireEvent.click(screen.getByText('OFF'));

    // Check if dispatch was called with the correct action
    expect(mockDispatch).toHaveBeenCalledWith({ type: 'TOGGLE_RADIO' });
  });

  test('changes frequency with tune buttons', () => {
    // Set radio to on for this test
    mockGameState.isRadioOn = true;

    render(<SimpleSliderRadioTuner />);

    // Find the increase button
    const increaseButton = screen.getByText('+0.1');

    // Click the increase button
    fireEvent.click(increaseButton);

    // Check if dispatch was called with the correct action
    expect(mockDispatch).toHaveBeenCalledWith({
      type: 'SET_FREQUENCY',
      payload: 90.1,
    });
  });

  test('does not cause infinite renders', () => {
    // This test verifies that the component doesn't cause infinite renders
    // by checking that the component doesn't log too many times

    // Set radio to on for this test
    mockGameState.isRadioOn = true;

    // Mock console.log to count how many times it's called
    const originalConsoleLog = console.log;
    const mockConsoleLog = jest.fn();
    console.log = mockConsoleLog;

    try {
      // Render the component
      const { rerender } = render(<SimpleSliderRadioTuner />);

      // Clear the mock to start fresh
      mockConsoleLog.mockClear();

      // Rerender the component multiple times
      rerender(<SimpleSliderRadioTuner />);
      rerender(<SimpleSliderRadioTuner />);
      rerender(<SimpleSliderRadioTuner />);

      // The component should not log more than a reasonable number of times
      // If it's logging hundreds of times, there's an infinite loop
      expect(mockConsoleLog.mock.calls.length).toBeLessThan(10);
    } finally {
      // Restore original console.log
      console.log = originalConsoleLog;
    }
  });

  test('handles frequency scanning', () => {
    // Set radio to on for this test
    mockGameState.isRadioOn = true;

    render(<SimpleSliderRadioTuner />);

    // Find and click the scan button
    const scanButton = screen.getByText('Scan');
    fireEvent.click(scanButton);

    // Check if the scan button text changes
    expect(screen.getByText('Stop Scan')).toBeInTheDocument();

    // Click stop scan
    fireEvent.click(screen.getByText('Stop Scan'));

    // Check if the scan button text changes back
    expect(screen.getByText('Scan')).toBeInTheDocument();
  });
});
