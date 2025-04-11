import React, { useEffect, useRef, useReducer } from 'react';
import Slider from 'rc-slider';
import 'rc-slider/assets/index.css';
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
import './RcSliderRadioTuner.css';

// Define a local state type for the radio tuner
interface RadioTunerState {
  localFrequency: number;
  signalStrength: number;
  currentSignalId: string | null;
  showMessage: boolean;
  staticIntensity: number;
  isScanning: boolean;
}

// Define action types for the reducer
type RadioTunerAction =
  | { type: 'SET_LOCAL_FREQUENCY'; payload: number }
  | { type: 'SET_SIGNAL_STRENGTH'; payload: number }
  | { type: 'SET_CURRENT_SIGNAL_ID'; payload: string | null }
  | { type: 'TOGGLE_MESSAGE' }
  | { type: 'SET_STATIC_INTENSITY'; payload: number }
  | { type: 'SET_SCANNING'; payload: boolean }
  | {
      type: 'UPDATE_SIGNAL_INFO';
      payload: { strength: number; signalId: string | null; intensity: number };
    };

// Create a reducer function for local state management
function radioTunerReducer(state: RadioTunerState, action: RadioTunerAction): RadioTunerState {
  switch (action.type) {
    case 'SET_LOCAL_FREQUENCY':
      return { ...state, localFrequency: action.payload };
    case 'SET_SIGNAL_STRENGTH':
      return { ...state, signalStrength: action.payload };
    case 'SET_CURRENT_SIGNAL_ID':
      return { ...state, currentSignalId: action.payload };
    case 'TOGGLE_MESSAGE':
      return { ...state, showMessage: !state.showMessage };
    case 'SET_STATIC_INTENSITY':
      return { ...state, staticIntensity: action.payload };
    case 'SET_SCANNING':
      return { ...state, isScanning: action.payload };
    case 'UPDATE_SIGNAL_INFO':
      return {
        ...state,
        signalStrength: action.payload.strength,
        currentSignalId: action.payload.signalId,
        staticIntensity: action.payload.intensity,
      };
    default:
      return state;
  }
}

interface RadioTunerProps {
  initialFrequency?: number;
  minFrequency?: number;
  maxFrequency?: number;
  onFrequencyChange?: (frequency: number) => void;
}

const SimpleSliderRadioTuner: React.FC<RadioTunerProps> = ({
  initialFrequency = 90.0,
  minFrequency = 88.0,
  maxFrequency = 108.0,
  onFrequencyChange,
}) => {
  const { state, dispatch: gameDispatch } = useGameState();
  const audio = useAudio();

  // Use a ref to store the frequency to avoid re-renders
  const frequencyRef = useRef<number>(
    state.currentFrequency !== undefined ? state.currentFrequency : initialFrequency
  );

  // Initialize local state with reducer
  const initialRadioState: RadioTunerState = {
    localFrequency: frequencyRef.current,
    signalStrength: 0,
    currentSignalId: null,
    showMessage: false,
    staticIntensity: 0.5,
    isScanning: false,
  };

  const [radioState, dispatch] = useReducer(radioTunerReducer, initialRadioState);
  const {
    localFrequency,
    signalStrength,
    currentSignalId,
    showMessage,
    staticIntensity,
    isScanning,
  } = radioState;

  const staticCanvasRef = useRef<HTMLCanvasElement>(null);
  const scanIntervalRef = useRef<number | null>(null);
  const isMountedRef = useRef<boolean>(true);

  // Initialize on mount
  useEffect(() => {
    // Set the mounted flag
    isMountedRef.current = true;

    // Initialize the game state if needed
    if (state.currentFrequency === undefined) {
      gameDispatch({ type: 'SET_FREQUENCY', payload: initialFrequency });
    } else {
      // Update our ref and local state with the current game state
      frequencyRef.current = state.currentFrequency;
      dispatch({ type: 'SET_LOCAL_FREQUENCY', payload: state.currentFrequency });
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
  }, []);

  // Sync with game state when it changes externally
  useEffect(() => {
    if (
      state.currentFrequency !== undefined &&
      Math.abs(state.currentFrequency - frequencyRef.current) > 0.01
    ) {
      frequencyRef.current = state.currentFrequency;
      if (isMountedRef.current) {
        dispatch({ type: 'SET_LOCAL_FREQUENCY', payload: state.currentFrequency });
      }
    }
  }, [state.currentFrequency]);

  // Process frequency changes and update audio
  useEffect(() => {
    // Only run this effect when the radio is on
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

      // Update UI state using the reducer
      dispatch({
        type: 'UPDATE_SIGNAL_INFO',
        payload: {
          strength,
          signalId: signal.messageId,
          intensity,
        },
      });

      // If this is a new signal discovery, add it to discovered frequencies
      if (!state.discoveredFrequencies.includes(signal.frequency)) {
        gameDispatch({ type: 'ADD_DISCOVERED_FREQUENCY', payload: signal.frequency });
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

      // Update UI state using the reducer
      dispatch({
        type: 'UPDATE_SIGNAL_INFO',
        payload: {
          strength: 0.1,
          signalId: null,
          intensity,
        },
      });

      audio.stopSignal();
      audio.playStaticNoise(intensity);
    }
  }, [state.isRadioOn, state.discoveredFrequencies, audio, gameDispatch, localFrequency]);

  // Handle frequency change from the slider
  const handleSliderChange = (value: number | number[]): void => {
    if (typeof value === 'number') {
      const roundedFreq = parseFloat(value.toFixed(1));

      // Update our ref first
      frequencyRef.current = roundedFreq;

      // Then update local state for UI
      dispatch({ type: 'SET_LOCAL_FREQUENCY', payload: roundedFreq });

      // Update game state
      gameDispatch({ type: 'SET_FREQUENCY', payload: roundedFreq });

      // Call the callback if provided
      if (onFrequencyChange) {
        onFrequencyChange(roundedFreq);
      }
    }
  };

  // Toggle message display
  const toggleMessage = (): void => {
    dispatch({ type: 'TOGGLE_MESSAGE' });
  };

  // Toggle frequency scanning
  const toggleScanning = (): void => {
    if (isScanning) {
      // Stop scanning
      dispatch({ type: 'SET_SCANNING', payload: false });
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    } else {
      // Start scanning
      dispatch({ type: 'SET_SCANNING', payload: true });
      scanIntervalRef.current = window.setInterval(() => {
        // Increment frequency by 0.1 MHz
        const newFreq = frequencyRef.current + 0.1;
        // If we reach the max frequency, loop back to min
        const nextFreq = newFreq > maxFrequency ? minFrequency : parseFloat(newFreq.toFixed(1));

        // Update our ref first
        frequencyRef.current = nextFreq;

        // Then update local state for UI (only if component is still mounted)
        if (isMountedRef.current) {
          dispatch({ type: 'SET_LOCAL_FREQUENCY', payload: nextFreq });
        }

        // Update game state
        gameDispatch({ type: 'SET_FREQUENCY', payload: nextFreq });

        // Call the callback if provided
        if (onFrequencyChange) {
          onFrequencyChange(nextFreq);
        }
      }, 300); // Scan speed in milliseconds
    }
  };

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
      const intensity = staticIntensity * 255;

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
  }, [state.isRadioOn]); // Only re-run when radio state changes

  // Get the current message
  const currentMessage = currentSignalId ? getMessage(currentSignalId) : undefined;

  // Calculate dial position based on current frequency (used for visualization)
  ((localFrequency - minFrequency) / (maxFrequency - minFrequency)) * 100;

  // Create marks for the slider
  const marks: Record<number, React.ReactNode> = {};
  for (let i = 0; i <= 10; i++) {
    const markerFrequency = minFrequency + (i * (maxFrequency - minFrequency)) / 10;
    if (i % 2 === 0) {
      marks[markerFrequency] = markerFrequency.toFixed(1);
    } else {
      marks[markerFrequency] = '';
    }
  }

  return (
    <div
      className="radio-tuner"
      data-testid="radio-tuner"
      aria-label="Radio Tuner. Use slider to adjust frequency."
    >
      <div className="radio-controls">
        <div className="power-button-container">
          <button
            className={`power-button ${state.isRadioOn ? 'on' : 'off'}`}
            onClick={() => gameDispatch({ type: 'TOGGLE_RADIO' })}
            aria-label={state.isRadioOn ? 'Turn radio off' : 'Turn radio on'}
          >
            {state.isRadioOn ? 'ON' : 'OFF'}
          </button>
        </div>

        <div
          className="frequency-display"
          aria-live="polite"
          aria-label={`Current frequency ${localFrequency.toFixed(1)} MHz`}
        >
          <span className="frequency-value">{localFrequency.toFixed(1)}</span>
          <span className="frequency-unit">MHz</span>
        </div>

        <div className="volume-control">
          <label htmlFor="volume-slider">Volume</label>
          <input
            id="volume-slider"
            type="range"
            min="0"
            max="1"
            step="0.1"
            value={audio.volume}
            onChange={(e) => audio.setVolume(parseFloat(e.target.value))}
            aria-label={`Volume control, current value ${audio.volume * 100}%`}
          />
          <button
            className={`mute-button ${audio.isMuted ? 'muted' : ''}`}
            onClick={audio.toggleMute}
            aria-label={audio.isMuted ? 'Unmute audio' : 'Mute audio'}
          >
            {audio.isMuted ? 'Unmute' : 'Mute'}
          </button>
        </div>

        <div className="noise-type-control">
          <label htmlFor="noise-type-select">Noise Type</label>
          <select
            id="noise-type-select"
            value={audio.currentNoiseType}
            onChange={(e) => audio.setNoiseType(e.target.value as NoiseType)}
            disabled={!state.isRadioOn}
            aria-label="Select noise type"
          >
            <option value={NoiseType.Pink}>Pink Noise</option>
            <option value={NoiseType.White}>White Noise</option>
            <option value={NoiseType.Brown}>Brown Noise</option>
          </select>
        </div>
      </div>

      {/* Static visualization canvas */}
      <div className="static-visualization-container">
        <canvas
          ref={staticCanvasRef}
          className="static-canvas"
          style={{
            opacity: state.isRadioOn ? staticIntensity : 0,
            pointerEvents: 'none',
          }}
          aria-hidden="true"
        />
        <div
          className="static-overlay"
          style={{
            opacity: state.isRadioOn ? 0.7 : 0,
          }}
        />
      </div>

      {/* RC Slider for frequency tuning */}
      <div className="rc-slider-container">
        <Slider
          min={minFrequency}
          max={maxFrequency}
          step={0.1}
          value={localFrequency}
          onChange={handleSliderChange}
          disabled={!state.isRadioOn || isScanning}
          marks={marks}
          styles={{
            rail: { backgroundColor: '#333', height: 10 },
            track: { backgroundColor: '#666', height: 10 },
            handle: {
              borderColor: '#007bff',
              height: 28,
              width: 28,
              marginTop: -9,
              backgroundColor: '#fff',
              boxShadow: '0 0 5px rgba(0, 123, 255, 0.5)',
            },
          }}
          dotStyle={{
            borderColor: '#666',
            height: 12,
            width: 2,
            marginLeft: -1,
            bottom: -6,
          }}
          activeDotStyle={{
            borderColor: '#007bff',
          }}
        />
      </div>

      <div className={`signal-strength-container ${!state.isRadioOn ? 'disabled' : ''}`}>
        <div className="signal-strength-label">
          <span>Signal Strength</span>
          <span className="signal-strength-value">
            {state.isRadioOn ? `${Math.round(signalStrength * 100)}%` : '0%'}
          </span>
        </div>
        <div
          className="signal-strength-meter"
          role="progressbar"
          aria-valuenow={signalStrength * 100}
          aria-valuemin={0}
          aria-valuemax={100}
          aria-label={`Signal strength ${Math.round(signalStrength * 100)}%`}
        >
          <div
            className="signal-strength-fill"
            style={{ width: `${state.isRadioOn ? signalStrength * 100 : 0}%` }}
          />
          <div className="signal-strength-ticks">
            {Array.from({ length: 10 }, (_, i) => (
              <div key={i} className="signal-strength-tick" />
            ))}
          </div>
          <div
            className="signal-strength-pulse"
            style={{ opacity: currentSignalId && signalStrength > 0.5 ? 1 : 0 }}
          />
        </div>
      </div>

      <div className="tuner-controls">
        <button
          className="tune-button decrease"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => {
            const newFreq = Math.max(
              minFrequency,
              parseFloat((frequencyRef.current - 0.1).toFixed(1))
            );
            frequencyRef.current = newFreq;
            dispatch({ type: 'SET_LOCAL_FREQUENCY', payload: newFreq });
            gameDispatch({ type: 'SET_FREQUENCY', payload: newFreq });
            if (onFrequencyChange) {
              onFrequencyChange(newFreq);
            }
          }}
          aria-label="Decrease frequency by 0.1 MHz"
        >
          -0.1
        </button>
        <button
          className="tune-button increase"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => {
            const newFreq = Math.min(
              maxFrequency,
              parseFloat((frequencyRef.current + 0.1).toFixed(1))
            );
            frequencyRef.current = newFreq;
            dispatch({ type: 'SET_LOCAL_FREQUENCY', payload: newFreq });
            gameDispatch({ type: 'SET_FREQUENCY', payload: newFreq });
            if (onFrequencyChange) {
              onFrequencyChange(newFreq);
            }
          }}
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
};

export default SimpleSliderRadioTuner;
