import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import RadioTuner from '../../../src/components/radio/RadioTuner';
import { GameStateProvider } from '../../../src/context/GameStateContext';
import { AudioProvider } from '../../../src/context/AudioContext';

describe('RadioTuner Component', () => {
  // Helper function to render with providers
  const renderWithProviders = (ui: React.ReactElement) => {
    return render(
      <GameStateProvider>
        <AudioProvider>
          {ui}
        </AudioProvider>
      </GameStateProvider>
    );
  };

  test('renders with default frequency', () => {
    renderWithProviders(<RadioTuner />);

    // Check if the component renders with the default frequency
    expect(screen.getByText('90.0')).toBeInTheDocument();
    expect(screen.getByText('MHz')).toBeInTheDocument();
  });

  test('renders with custom initial frequency', () => {
    renderWithProviders(<RadioTuner initialFrequency={95.5} />);

    // Check if the component renders with the custom frequency
    expect(screen.getByText('95.5')).toBeInTheDocument();
  });

  test('frequency changes when clicking tune buttons', () => {
    renderWithProviders(<RadioTuner initialFrequency={100.0} />);

    // Turn on the radio first
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Get the tune buttons
    const decreaseButton = screen.getByText('-0.1');
    const increaseButton = screen.getByText('+0.1');

    // Click the decrease button
    fireEvent.click(decreaseButton);
    expect(screen.getByText('99.9')).toBeInTheDocument();

    // Click the increase button
    fireEvent.click(increaseButton);
    expect(screen.getByText('100.0')).toBeInTheDocument();
  });

  test('calls onFrequencyChange when frequency changes', () => {
    const mockOnFrequencyChange = jest.fn();
    renderWithProviders(
      <RadioTuner
        initialFrequency={100.0}
        onFrequencyChange={mockOnFrequencyChange}
      />
    );

    // Turn on the radio first
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Get the tune button
    const decreaseButton = screen.getByText('-0.1');

    // Click the decrease button
    fireEvent.click(decreaseButton);

    // Check if the callback was called with the new frequency
    expect(mockOnFrequencyChange).toHaveBeenCalledWith(99.9);
  });

  test('respects min and max frequency limits', () => {
    renderWithProviders(
      <RadioTuner
        initialFrequency={90.0}
        minFrequency={90.0}
        maxFrequency={92.0}
      />
    );

    // Turn on the radio first
    const powerButton = screen.getByText('OFF');
    fireEvent.click(powerButton);

    // Get the tune buttons
    const decreaseButton = screen.getByText('-0.1');
    const increaseButton = screen.getByText('+0.1');

    // Try to decrease below minimum
    fireEvent.click(decreaseButton);
    expect(screen.getByText('90.0')).toBeInTheDocument(); // Should not change

    // Increase several times
    fireEvent.click(increaseButton); // 90.1
    fireEvent.click(increaseButton); // 90.2
    fireEvent.click(increaseButton); // 90.3

    // Check if the frequency changed
    expect(screen.getByText('90.3')).toBeInTheDocument();

    // Increase many more times to reach max
    for (let i = 0; i < 20; i++) {
      fireEvent.click(increaseButton);
    }

    // Should not exceed maximum
    expect(screen.getByText('92.0')).toBeInTheDocument();
  });

  test('toggles radio power', () => {
    renderWithProviders(<RadioTuner />);

    // Radio should be off initially
    const powerButton = screen.getByText('OFF');

    // Turn on the radio
    fireEvent.click(powerButton);
    expect(screen.getByText('ON')).toBeInTheDocument();

    // Turn off the radio
    fireEvent.click(screen.getByText('ON'));
    expect(screen.getByText('OFF')).toBeInTheDocument();
  });

  test('controls volume', () => {
    renderWithProviders(<RadioTuner />);

    // Find the volume slider
    const volumeSlider = screen.getByLabelText('Volume');

    // Change the volume
    fireEvent.change(volumeSlider, { target: { value: '0.8' } });

    // Test mute button
    const muteButton = screen.getByText('Mute');
    fireEvent.click(muteButton);
    expect(screen.getByText('Unmute')).toBeInTheDocument();
  });
});
