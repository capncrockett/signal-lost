import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import RadioTuner from '../../../src/components/radio/RadioTuner';

describe('RadioTuner Component', () => {
  test('renders with default frequency', () => {
    render(<RadioTuner />);

    // Check if the component renders with the default frequency
    expect(screen.getByText('90.0')).toBeInTheDocument();
    expect(screen.getByText('MHz')).toBeInTheDocument();
  });

  test('renders with custom initial frequency', () => {
    render(<RadioTuner initialFrequency={95.5} />);

    // Check if the component renders with the custom frequency
    expect(screen.getByText('95.5')).toBeInTheDocument();
  });

  test('frequency changes when clicking tune buttons', () => {
    render(<RadioTuner initialFrequency={100.0} />);

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
    render(
      <RadioTuner
        initialFrequency={100.0}
        onFrequencyChange={mockOnFrequencyChange}
      />
    );

    // Get the tune button
    const decreaseButton = screen.getByText('-0.1');

    // Click the decrease button
    fireEvent.click(decreaseButton);

    // Check if the callback was called with the new frequency
    expect(mockOnFrequencyChange).toHaveBeenCalledWith(99.9);
  });

  test('respects min and max frequency limits', () => {
    render(
      <RadioTuner
        initialFrequency={90.0}
        minFrequency={90.0}
        maxFrequency={92.0}
      />
    );

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
});
