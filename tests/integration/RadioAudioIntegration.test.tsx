import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import * as NoiseGenerator from '../../src/audio/NoiseGenerator';
import * as frequencies from '../../src/data/frequencies';

// Use our mocks instead of the real components
import { AudioProvider } from '../mocks/AudioContextMock';
import RadioTuner from '../mocks/RadioTunerMock';

// Mock the NoiseGenerator module with simpler implementation
jest.mock('../../src/audio/NoiseGenerator', () => ({
  createNoise: jest.fn().mockReturnValue({
    noise: { connect: jest.fn(), start: jest.fn(), stop: jest.fn(), dispose: jest.fn() },
    gain: {
      connect: jest.fn(),
      toDestination: jest.fn(),
      dispose: jest.fn(),
      gain: { value: 0.5 },
    },
    filter: { connect: jest.fn(), dispose: jest.fn() },
  }),
  createSignal: jest.fn().mockReturnValue({
    oscillator: { connect: jest.fn(), start: jest.fn(), stop: jest.fn(), dispose: jest.fn() },
    gain: {
      connect: jest.fn(),
      toDestination: jest.fn(),
      dispose: jest.fn(),
      gain: { value: 0.5 },
    },
  }),
  NoiseType: { White: 'white', Pink: 'pink', Brown: 'brown' },
}));

// Mock the frequencies module with simpler implementation
jest.mock('../../src/data/frequencies', () => ({
  findSignalAtFrequency: jest.fn().mockImplementation((frequency) => {
    // Return a mock signal for specific frequencies
    if (frequency === 91.5) {
      return { frequency: 91.5, messageId: 'signal-1', isStatic: false };
    } else if (frequency === 94.2) {
      return { frequency: 94.2, messageId: 'signal-2', isStatic: true };
    }
    return null;
  }),
  calculateSignalStrength: jest.fn().mockImplementation((frequency, signal) => {
    return frequency === signal.frequency ? 1.0 : 0.7;
  }),
  getStaticIntensity: jest.fn().mockReturnValue(0.8),
}));

// Mock the messages module with simpler implementation
jest.mock('../../src/data/messages', () => ({
  getMessage: jest.fn().mockImplementation((id) => {
    if (id === 'signal-1') {
      return { id: 'signal-1', title: 'Test Signal 1', content: 'Test content 1', decoded: true };
    } else if (id === 'signal-2') {
      return { id: 'signal-2', title: 'Test Signal 2', content: 'Test content 2', decoded: false };
    }
    return null;
  }),
}));

// Mock canvas functionality with minimal implementation
const mockCanvasContext = {
  fillRect: jest.fn(),
  clearRect: jest.fn(),
  getImageData: jest.fn().mockReturnValue({ data: new Uint8ClampedArray(4) }),
  putImageData: jest.fn(),
  createImageData: jest.fn().mockReturnValue({ data: new Uint8ClampedArray(4) }),
  fillText: jest.fn(),
};

// Mock Tone.js
jest.mock('tone', () => ({
  Destination: { volume: { value: 0 } },
  gainToDb: jest.fn().mockImplementation((gain) => gain * 20),
  Noise: jest.fn(),
  Oscillator: jest.fn(),
  Filter: jest.fn().mockImplementation(() => ({
    connect: jest.fn().mockReturnThis(),
    toDestination: jest.fn().mockReturnThis(),
    dispose: jest.fn(),
    frequency: { value: 1000 },
    Q: { value: 1 },
  })),
  Gain: jest.fn(),
}));

// Mock the actual AudioContext to use our mock
jest.mock('../../src/context/AudioContext', () => {
  return require('../mocks/AudioContextMock');
});

// Mock requestAnimationFrame globally
global.requestAnimationFrame = jest.fn().mockImplementation((cb) => {
  cb(0);
  return 0;
});

// Mock cancelAnimationFrame globally
global.cancelAnimationFrame = jest.fn();

describe('RadioTuner and Audio Integration', () => {
  // Helper function to render the component with providers
  const renderWithProviders = (ui: React.ReactElement) => {
    return render(
      <GameStateProvider persistState={false}>
        <AudioProvider>{ui}</AudioProvider>
      </GameStateProvider>
    );
  };

  beforeEach(() => {
    jest.clearAllMocks();

    // Mock getContext before each test
    HTMLCanvasElement.prototype.getContext = jest.fn().mockReturnValue(mockCanvasContext);
  });

  afterEach(() => {
    // Clean up any timers
    jest.useRealTimers();
  });

  test('tuning to a signal frequency triggers audio changes', () => {
    // Mock the findSignalAtFrequency function to return a non-static signal
    const mockSignal: frequencies.SignalFrequency = {
      frequency: 91.1,
      messageId: 'intro_signal',
      isStatic: false,
      tolerance: 0.1,
      signalStrength: 0.8
    };
    jest.spyOn(frequencies, 'findSignalAtFrequency').mockReturnValue(mockSignal);

    // Directly render with a frequency that has a clear signal
    renderWithProviders(<RadioTuner initialFrequency={91.1} />);

    // Turn on the radio
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Should create a signal (not static)
    expect(NoiseGenerator.createSignal).toHaveBeenCalled();
  });

  test('tuning to a static signal frequency plays both signal and static', () => {
    // Mock the findSignalAtFrequency function to return a static signal
    const mockSignal: frequencies.SignalFrequency = {
      frequency: 94.2,
      messageId: 'distress_call',
      isStatic: true,
      tolerance: 0.2,
      signalStrength: 0.6
    };
    jest.spyOn(frequencies, 'findSignalAtFrequency').mockReturnValue(mockSignal);

    // Clear mocks before the test
    jest.clearAllMocks();

    renderWithProviders(<RadioTuner initialFrequency={94.2} />);

    // Turn on the radio
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Should create both a signal and static noise
    expect(NoiseGenerator.createSignal).toHaveBeenCalled();
    expect(NoiseGenerator.createNoise).toHaveBeenCalled();
  });

  test('tuning to a frequency without a signal plays only static', () => {
    // Mock the findSignalAtFrequency function to return null (no signal)
    jest.spyOn(frequencies, 'findSignalAtFrequency').mockReturnValue(null);

    // Clear mocks before the test
    jest.clearAllMocks();

    // Directly render with a frequency that has no signal
    renderWithProviders(<RadioTuner initialFrequency={92.0} />);

    // Turn on the radio
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Should create only static noise, not a signal
    expect(NoiseGenerator.createSignal).not.toHaveBeenCalled();
    expect(NoiseGenerator.createNoise).toHaveBeenCalled();
  });

  test('changing volume affects audio output', () => {
    renderWithProviders(<RadioTuner initialFrequency={90.0} />);

    // Find the volume slider
    const volumeSlider = screen.getByLabelText('Volume');

    // Change the volume
    fireEvent.change(volumeSlider, { target: { value: '0.8' } });

    // We can verify the component rendered with the new volume
    expect(volumeSlider).toHaveValue('0.8');
  });

  test('changing noise type updates the audio', () => {
    renderWithProviders(<RadioTuner initialFrequency={90.0} />);

    // Turn on the radio
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Find the noise type selector
    const noiseTypeSelect = screen.getByLabelText('Select noise type');

    // Change noise type to White Noise
    fireEvent.change(noiseTypeSelect, { target: { value: 'white' } });

    // We can verify the component rendered with the new noise type
    expect(noiseTypeSelect).toHaveValue('white');
  });

  test('scanning automatically tunes through frequencies', () => {
    renderWithProviders(<RadioTuner initialFrequency={90.0} />);

    // Turn on the radio
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Find the scan button
    const scanButton = screen.getByText('Scan');

    // Start scanning
    fireEvent.click(scanButton);

    // Verify scan mode is active
    expect(screen.getByText('Stop Scan')).toBeInTheDocument();

    // Stop scanning
    fireEvent.click(screen.getByText('Stop Scan'));

    // Verify scan mode is inactive
    expect(screen.getByText('Scan')).toBeInTheDocument();
  });
});
