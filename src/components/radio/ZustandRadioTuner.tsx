import React, { useEffect, useRef, memo, useCallback } from 'react';
import { useGameState } from '../../context/GameStateContext';
import { useAudio } from '../../context/AudioContext';
import { getMessage } from '../../data/messages';
import MessageDisplay from '../narrative/MessageDisplay';
import { useRadioStore } from '../../store/radioStore';
import { processFrequency } from '../../utils/frequencyProcessor';
import './RadioTuner.css';
import './BasicRadioTuner.css';

interface RadioTunerProps {
  initialFrequency?: number;
  minFrequency?: number;
  maxFrequency?: number;
  onFrequencyChange?: (frequency: number) => void;
}

const ZustandRadioTuner: React.FC<RadioTunerProps> = memo(({
  initialFrequency = 90.0,
  minFrequency = 88.0,
  maxFrequency = 108.0,
  onFrequencyChange,
}) => {
  // For debugging - only in development
  if (process.env.NODE_ENV === 'development') {
    console.log('ZustandRadioTuner rendering', new Date().toISOString());
  }

  // Context
  const { state, dispatch } = useGameState();
  const audio = useAudio();

  // Refs that don't trigger re-renders
  const staticCanvasRef = useRef<HTMLCanvasElement>(null);
  const scanIntervalRef = useRef<number | null>(null);
  const animationFrameRef = useRef<number | null>(null);

  // Zustand state
  const {
    showMessage,
    isScanning,
    signalStrength,
    staticIntensity,
    currentSignalId,
    toggleMessage,
    setIsScanning
  } = useRadioStore();

  // Initialize on mount
  useEffect(() => {
    // Initialize the game state if needed
    if (state.currentFrequency === undefined) {
      dispatch({ type: 'SET_FREQUENCY', payload: initialFrequency });
    }

    // Cleanup on unmount
    return () => {
      // Clean up all audio resources
      audio.stopSignal();
      audio.stopStaticNoise();

      // Clear any remaining intervals
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }

      // Cancel any animation frames
      if (animationFrameRef.current !== null) {
        cancelAnimationFrame(animationFrameRef.current);
        animationFrameRef.current = null;
      }
    };
  }, []); // Empty dependency array means this runs once on mount

  // Process frequency changes and update audio
  useEffect(() => {
    // Only process if the radio is on
    if (!state.isRadioOn) {
      audio.stopSignal();
      audio.stopStaticNoise();
      return;
    }

    const addDiscoveredFrequency = (freq: number) => {
      dispatch({ type: 'ADD_DISCOVERED_FREQUENCY', payload: freq });
    };

    processFrequency(
      state.currentFrequency,
      state.isRadioOn,
      audio,
      state.discoveredFrequencies,
      addDiscoveredFrequency
    );

    // We don't need to include state.discoveredFrequencies in the dependency array
    // because it's only used inside the processFrequency function to check if a frequency
    // has been discovered, and we're already handling that with the addDiscoveredFrequency callback
  }, [state.isRadioOn, state.currentFrequency, audio, dispatch]);

  // Draw static visualization
  useEffect(() => {
    // Skip if radio is off or canvas is not available
    if (!state.isRadioOn || !staticCanvasRef.current) {
      return;
    }

    const canvas = staticCanvasRef.current;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Set canvas dimensions
    if (canvas.width !== canvas.clientWidth || canvas.height !== canvas.clientHeight) {
      canvas.width = canvas.clientWidth;
      canvas.height = canvas.clientHeight;
    }

    let isActive = true;

    // Store the current static intensity in a ref to avoid re-renders
    const currentIntensity = staticIntensity;

    const drawStatic = () => {
      if (!isActive || !state.isRadioOn) return;

      // Clear canvas
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Draw static noise
      const intensity = currentIntensity * 255;

      // Optimize by drawing larger blocks
      for (let i = 0; i < canvas.width; i += 4) {
        for (let j = 0; j < canvas.height; j += 4) {
          const noiseValue = Math.random() * intensity;
          ctx.fillStyle = `rgba(${noiseValue}, ${noiseValue}, ${noiseValue}, 0.5)`;
          ctx.fillRect(i, j, 4, 4);
        }
      }

      // Continue animation
      animationFrameRef.current = requestAnimationFrame(drawStatic);
    };

    // Start animation
    animationFrameRef.current = requestAnimationFrame(drawStatic);

    // Cleanup
    return () => {
      isActive = false;
      if (animationFrameRef.current) {
        cancelAnimationFrame(animationFrameRef.current);
        animationFrameRef.current = null;
      }
    };

    // Only re-run this effect when the radio is turned on/off
    // We don't need to include staticIntensity in the dependency array
    // because we're using the currentIntensity variable to capture its value
  }, [state.isRadioOn]);

  // Handle scanning
  useEffect(() => {
    // Only start scanning if both conditions are met
    const shouldScan = isScanning && state.isRadioOn;

    if (shouldScan) {
      // Start scanning
      scanIntervalRef.current = window.setInterval(() => {
        // Increment frequency by 0.1 MHz
        const newFreq = state.currentFrequency + 0.1;
        // If we reach the max frequency, loop back to min
        const nextFreq = newFreq > maxFrequency ? minFrequency : parseFloat(newFreq.toFixed(1));

        // Update game state
        dispatch({ type: 'SET_FREQUENCY', payload: nextFreq });

        // Call the callback if provided
        if (onFrequencyChange) {
          onFrequencyChange(nextFreq);
        }
      }, 300); // Scan speed in milliseconds
    }

    // Cleanup function that runs when the component unmounts or when dependencies change
    return () => {
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    };
  }, [isScanning, state.isRadioOn, state.currentFrequency, dispatch, maxFrequency, minFrequency, onFrequencyChange]);

  // Toggle frequency scanning - memoized to prevent unnecessary re-renders
  const toggleScanning = useCallback(() => {
    setIsScanning(!isScanning);
  }, [isScanning, setIsScanning]);

  // Change frequency by a specific amount - memoized to prevent unnecessary re-renders
  const changeFrequency = useCallback((amount: number) => {
    const currentFreq = state.currentFrequency;
    const newFreq = Math.max(
      minFrequency,
      Math.min(maxFrequency, parseFloat((currentFreq + amount).toFixed(1)))
    );

    // Update game state
    dispatch({ type: 'SET_FREQUENCY', payload: newFreq });

    // Call the callback if provided
    if (onFrequencyChange) {
      onFrequencyChange(newFreq);
    }
  }, [state.currentFrequency, minFrequency, maxFrequency, dispatch, onFrequencyChange]);

  // Get the current message
  const currentMessage = currentSignalId ? getMessage(currentSignalId) : undefined;

  // Calculate the percentage for the frequency slider
  const frequencyPercentage =
    ((state.currentFrequency - minFrequency) / (maxFrequency - minFrequency)) * 100;

  return (
    <div className="radio-tuner">
      <div className="radio-display">
        <div className="frequency-display">
          <span className="frequency-value">{state.currentFrequency.toFixed(1)}</span>
          <span className="frequency-unit">MHz</span>
        </div>
        <div className="signal-strength-meter">
          <div
            className="signal-strength-fill"
            style={{ width: `${signalStrength * 100}%` }}
          ></div>
        </div>
        <canvas
          ref={staticCanvasRef}
          className="static-visualization"
          style={{ opacity: staticIntensity }}
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
              aria-label={`Frequency: ${state.currentFrequency.toFixed(1)} MHz`}
            ></div>
          </div>
        </div>

        {/* Signal strength indicator */}
        <div className="signal-strength-indicator">
          <div className="signal-strength-label">
            <span>Signal Strength</span>
            <span className="signal-strength-value">
              {Math.round(signalStrength * 100)}%
            </span>
          </div>
          <div className="signal-strength-meter">
            <div
              className="signal-strength-fill"
              style={{ width: `${signalStrength * 100}%` }}
            ></div>
          </div>
        </div>
      </div>

      <div className="tuner-controls">
        <button
          className="tune-button decrease"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => changeFrequency(-0.1)}
          aria-label="Decrease frequency by 0.1 MHz"
        >
          -0.1
        </button>
        <button
          className="tune-button increase"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => changeFrequency(0.1)}
          aria-label="Increase frequency by 0.1 MHz"
        >
          +0.1
        </button>
        <button
          className="scan-button"
          disabled={!state.isRadioOn}
          onClick={toggleScanning}
          aria-label={isScanning ? 'Stop scanning' : 'Start scanning'}
        >
          {isScanning ? 'Stop Scan' : 'Scan'}
        </button>
      </div>

      {/* Message display */}
      {currentMessage && (
        <div className="message-container">
          <button
            className="message-button"
            onClick={toggleMessage}
            disabled={!state.isRadioOn || !currentMessage}
            aria-label={showMessage ? 'Hide message' : 'Show message'}
          >
            {showMessage ? 'Hide Message' : 'Show Message'}
          </button>
          {showMessage && currentMessage && (
            <MessageDisplay message={currentMessage} isVisible={true} />
          )}
        </div>
      )}
    </div>
  );
}));  // Close the memo() call

export default ZustandRadioTuner;
