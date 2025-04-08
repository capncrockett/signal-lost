import React, { useState, useEffect } from 'react';
import './RadioTuner.css';

interface RadioTunerProps {
  initialFrequency?: number;
  minFrequency?: number;
  maxFrequency?: number;
  onFrequencyChange?: (frequency: number) => void;
}

const RadioTuner: React.FC<RadioTunerProps> = ({
  initialFrequency = 90.0,
  minFrequency = 88.0,
  maxFrequency = 108.0,
  onFrequencyChange,
}) => {
  const [frequency, setFrequency] = useState<number>(initialFrequency);
  const [isDragging, setIsDragging] = useState<boolean>(false);
  const [signalStrength, setSignalStrength] = useState<number>(0);

  // Update frequency when dragging the dial
  const handleMouseDown = (): void => {
    setIsDragging(true);
  };

  const handleMouseUp = (): void => {
    setIsDragging(false);
  };

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>): void => {
    if (!isDragging) return;

    const container = e.currentTarget;
    const rect = container.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const width = rect.width;
    const percentage = Math.max(0, Math.min(1, x / width));
    
    const newFrequency = minFrequency + percentage * (maxFrequency - minFrequency);
    setFrequency(parseFloat(newFrequency.toFixed(1)));
  };

  // Simulate signal strength based on frequency
  useEffect(() => {
    // This is a placeholder for actual signal detection logic
    // In a real implementation, this would check if the frequency matches
    // any of the valid signal frequencies
    const isValidSignal = Math.random() > 0.7;
    const strength = isValidSignal ? Math.random() * 0.8 + 0.2 : Math.random() * 0.2;
    
    setSignalStrength(strength);
    
    if (onFrequencyChange) {
      onFrequencyChange(frequency);
    }
  }, [frequency, onFrequencyChange]);

  // Calculate dial position based on current frequency
  const dialPosition = ((frequency - minFrequency) / (maxFrequency - minFrequency)) * 100;

  // Add global mouse event listeners for dragging
  useEffect(() => {
    const handleGlobalMouseUp = (): void => {
      setIsDragging(false);
    };

    if (isDragging) {
      window.addEventListener('mouseup', handleGlobalMouseUp);
    }

    return () => {
      window.removeEventListener('mouseup', handleGlobalMouseUp);
    };
  }, [isDragging]);

  return (
    <div className="radio-tuner" data-testid="radio-tuner">
      <div className="frequency-display">
        <span className="frequency-value">{frequency.toFixed(1)}</span>
        <span className="frequency-unit">MHz</span>
      </div>
      
      <div 
        className="tuner-dial-container"
        onMouseDown={handleMouseDown}
        onMouseUp={handleMouseUp}
        onMouseMove={handleMouseMove}
      >
        <div className="tuner-dial-track">
          <div 
            className="tuner-dial-knob"
            style={{ left: `${dialPosition}%` }}
          />
        </div>
      </div>
      
      <div className="signal-strength-meter">
        <div 
          className="signal-strength-fill"
          style={{ width: `${signalStrength * 100}%` }}
        />
      </div>
      
      <div className="tuner-controls">
        <button 
          className="tune-button decrease"
          onClick={() => setFrequency(prev => Math.max(minFrequency, prev - 0.1))}
        >
          -0.1
        </button>
        <button 
          className="tune-button increase"
          onClick={() => setFrequency(prev => Math.min(maxFrequency, prev + 0.1))}
        >
          +0.1
        </button>
      </div>
    </div>
  );
};

export default RadioTuner;
