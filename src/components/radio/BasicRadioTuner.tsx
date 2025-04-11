import React, { useEffect, useRef } from 'react';
import { useAudio } from '../../context/AudioContext';
import { useGameState } from '../../context/GameStateContext';
import {
  findSignalAtFrequency,
  calculateSignalStrength,
  getStaticIntensity,
} from '../../data/frequencies';
import { getMessage } from '../../data/messages';
import MessageDisplay from '../narrative/MessageDisplay';
import './RadioTuner.css';
import './BasicRadioTuner.css';

interface RadioTunerProps {
  initialFrequency?: number;
  minFrequency?: number;
  maxFrequency?: number;
  onFrequencyChange?: (frequency: number) => void;
}

const BasicRadioTuner: React.FC<RadioTunerProps> = ({
  initialFrequency = 90.0,
  minFrequency = 88.0,
  maxFrequency = 108.0,
  onFrequencyChange,
}) => {
  const { state, dispatch } = useGameState();
  const audio = useAudio();

  // Use refs to avoid re-renders
  const frequencyRef = useRef<number>(
    state.currentFrequency !== undefined ? state.currentFrequency : initialFrequency
  );
  const signalStrengthRef = useRef<number>(0);
  const currentSignalIdRef = useRef<string | null>(null);
  const showMessageRef = useRef<boolean>(false);
  const staticIntensityRef = useRef<number>(0.5);
  const isScanningRef = useRef<boolean>(false);
  const scanIntervalRef = useRef<number | null>(null);
  const isMountedRef = useRef<boolean>(true);
  const staticCanvasRef = useRef<HTMLCanvasElement>(null);

  // Initialize on mount
  useEffect(() => {
    // Set the mounted flag
    isMountedRef.current = true;

    // Initialize the game state if needed
    if (state.currentFrequency === undefined) {
      dispatch({ type: 'SET_FREQUENCY', payload: initialFrequency });
    } else {
      // Update our ref with the current game state
      frequencyRef.current = state.currentFrequency;
    }

    // Cleanup on unmount
    return () => {
      isMountedRef.current = false;

      // Clean up all audio resources
      audio.stopSignal();
      audio.stopStaticNoise();

      // Clear any remaining intervals
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    };
  }, [audio, dispatch, initialFrequency, state.currentFrequency]);

  // Sync with game state when it changes externally
  useEffect(() => {
    if (
      state.currentFrequency !== undefined &&
      Math.abs(state.currentFrequency - frequencyRef.current) > 0.01
    ) {
      frequencyRef.current = state.currentFrequency;
      // Force a re-render
      forceRender();
    }
  }, [state.currentFrequency, forceRender]);

  // Process frequency changes and update audio
  useEffect(() => {
    // Only process if the radio is on
    if (!state.isRadioOn) {
      audio.stopSignal();
      audio.stopStaticNoise();
      return;
    }

    // Get the current frequency
    const frequency = frequencyRef.current;

    // Check if there's a signal at this frequency
    const signal = findSignalAtFrequency(frequency);

    if (signal) {
      // Calculate signal strength based on how close we are to the exact frequency
      const strength = calculateSignalStrength(frequency, signal);

      // Calculate static intensity based on signal strength
      const intensity = signal.isStatic ? 1 - strength : (1 - strength) * 0.5;

      // Update refs
      signalStrengthRef.current = strength;
      currentSignalIdRef.current = signal.messageId;
      staticIntensityRef.current = intensity;

      // If this is a new signal discovery, add it to discovered frequencies
      if (!state.discoveredFrequencies.includes(signal.frequency)) {
        dispatch({ type: 'ADD_DISCOVERED_FREQUENCY', payload: signal.frequency });
      }

      // Play appropriate audio
      if (signal.isStatic) {
        // Play static with the signal mixed in
        audio.playStaticNoise(intensity);
        audio.playSignal(signal.frequency * 10, strength * 0.5); // Scale up for audible range
      } else {
        // Play a clear signal
        audio.stopStaticNoise();
        audio.playSignal(signal.frequency * 10); // Scale up for audible range
      }
    } else {
      // No signal found, just play static
      const intensity = getStaticIntensity(frequency);

      // Update refs
      staticIntensityRef.current = intensity;
      signalStrengthRef.current = 0.1; // Low signal strength
      currentSignalIdRef.current = null;

      audio.stopSignal();
      audio.playStaticNoise(intensity);
    }

    // Force a re-render to update the UI
    forceRender();
  }, [state.isRadioOn, state.discoveredFrequencies, audio, dispatch]);

  // Draw static visualization with performance optimizations
  useEffect(() => {
    // Skip if radio is off or canvas is not available
    if (!state.isRadioOn || !staticCanvasRef.current) return;

    const canvas = staticCanvasRef.current;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Set canvas dimensions
    if (canvas.width !== canvas.clientWidth || canvas.height !== canvas.clientHeight) {
      canvas.width = canvas.clientWidth;
      canvas.height = canvas.clientHeight;
    }

    // Simple animation function that doesn't cause re-renders
    let animationId: number;

    const drawStatic = (): void => {
      if (!isMountedRef.current || !state.isRadioOn) return;

      // Clear canvas
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Draw static noise
      const intensity = staticIntensityRef.current * 255;

      for (let i = 0; i < canvas.width; i += 4) {
        for (let j = 0; j < canvas.height; j += 4) {
          const noiseValue = Math.random() * intensity;
          ctx.fillStyle = `rgba(${noiseValue}, ${noiseValue}, ${noiseValue}, 0.5)`;
          ctx.fillRect(i, j, 4, 4);
        }
      }

      // Continue animation
      animationId = requestAnimationFrame(drawStatic);
    };

    // Start animation
    animationId = requestAnimationFrame(drawStatic);

    // Cleanup
    return () => {
      if (animationId) {
        cancelAnimationFrame(animationId);
      }
    };
  }, [state.isRadioOn, forceRender]);

  // Force a re-render without using state
  const [, updateState] = React.useState<object>({});
  const forceRender = React.useCallback(() => updateState({}), []);

  // Toggle message display
  const toggleMessage = (): void => {
    showMessageRef.current = !showMessageRef.current;
    forceRender();
  };

  // Toggle frequency scanning
  const toggleScanning = (): void => {
    if (isScanningRef.current) {
      // Stop scanning
      isScanningRef.current = false;
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    } else {
      // Start scanning
      isScanningRef.current = true;
      scanIntervalRef.current = window.setInterval(() => {
        // Increment frequency by 0.1 MHz
        const newFreq = frequencyRef.current + 0.1;
        // If we reach the max frequency, loop back to min
        const nextFreq = newFreq > maxFrequency ? minFrequency : parseFloat(newFreq.toFixed(1));

        // Update our ref first
        frequencyRef.current = nextFreq;

        // Update game state
        dispatch({ type: 'SET_FREQUENCY', payload: nextFreq });

        // Call the callback if provided
        if (onFrequencyChange) {
          onFrequencyChange(nextFreq);
        }

        // Force a re-render
        forceRender();
      }, 300); // Scan speed in milliseconds
    }

    // Force a re-render
    forceRender();
  };

  // Change frequency by a specific amount
  const changeFrequency = (amount: number): void => {
    const currentFreq = frequencyRef.current;
    const newFreq = Math.max(
      minFrequency,
      Math.min(maxFrequency, parseFloat((currentFreq + amount).toFixed(1)))
    );

    // Update our ref
    frequencyRef.current = newFreq;

    // Update game state
    dispatch({ type: 'SET_FREQUENCY', payload: newFreq });

    // Call the callback if provided
    if (onFrequencyChange) {
      onFrequencyChange(newFreq);
    }

    // Force a re-render
    forceRender();
  };

  // Get the current message
  const currentMessage = currentSignalIdRef.current
    ? getMessage(currentSignalIdRef.current)
    : undefined;

  // Calculate the percentage for the frequency slider
  const frequencyPercentage =
    ((frequencyRef.current - minFrequency) / (maxFrequency - minFrequency)) * 100;

  return (
    <div className="radio-tuner">
      <div className="radio-display">
        <div className="frequency-display">
          <span className="frequency-value">{frequencyRef.current.toFixed(1)}</span>
          <span className="frequency-unit">MHz</span>
        </div>
        <div className="signal-strength-meter">
          <div
            className="signal-strength-fill"
            style={{ width: `${signalStrengthRef.current * 100}%` }}
          ></div>
        </div>
        <canvas
          ref={staticCanvasRef}
          className="static-visualization"
          style={{ opacity: staticIntensityRef.current }}
        ></canvas>
      </div>

      <div className="radio-controls">
        <div className="power-button-container">
          <button
            className={`power-button ${state.isRadioOn ? 'on' : 'off'}`}
            onClick={() => dispatch({ type: 'TOGGLE_RADIO' })}
            aria-label={state.isRadioOn ? 'Turn radio off' : 'Turn radio on'}
          >
            {state.isRadioOn ? 'ON' : 'OFF'}
          </button>
        </div>
      </div>

      <div className="frequency-slider-container">
        <div className="frequency-slider">
          <div className="frequency-track">
            <div
              className="frequency-handle"
              style={{ left: `${frequencyPercentage}%` }}
              aria-label={`Frequency: ${frequencyRef.current.toFixed(1)} MHz`}
            ></div>
          </div>
        </div>
      </div>

      <div className="tuner-controls">
        <button
          className="tune-button decrease"
          disabled={!state.isRadioOn || isScanningRef.current}
          onClick={() => changeFrequency(-0.1)}
          aria-label="Decrease frequency by 0.1 MHz"
        >
          -0.1
        </button>
        <button
          className="tune-button increase"
          disabled={!state.isRadioOn || isScanningRef.current}
          onClick={() => changeFrequency(0.1)}
          aria-label="Increase frequency by 0.1 MHz"
        >
          +0.1
        </button>
        <button
          className="scan-button"
          disabled={!state.isRadioOn}
          onClick={toggleScanning}
          aria-label={isScanningRef.current ? 'Stop scanning' : 'Start scanning'}
        >
          {isScanningRef.current ? 'Stop Scan' : 'Scan'}
        </button>
      </div>

      {/* Message display */}
      {currentMessage && (
        <div className="message-container">
          <button
            className="message-button"
            onClick={toggleMessage}
            disabled={!state.isRadioOn || !currentMessage}
            aria-label={showMessageRef.current ? 'Hide message' : 'Show message'}
          >
            {showMessageRef.current ? 'Hide Message' : 'Show Message'}
          </button>
          {showMessageRef.current && currentMessage && (
            <MessageDisplay message={currentMessage} isVisible={true} />
          )}
        </div>
      )}
    </div>
  );
};

export default BasicRadioTuner;
