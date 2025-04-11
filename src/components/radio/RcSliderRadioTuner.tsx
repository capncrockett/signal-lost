import React, { useState, useEffect, useRef } from 'react';
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
import { NoiseType } from '../../audio/NoiseType';
import './RadioTuner.css';
import './RcSliderRadioTuner.css'; // We'll create this file for custom styling

interface RadioTunerProps {
  initialFrequency?: number;
  minFrequency?: number;
  maxFrequency?: number;
  onFrequencyChange?: (frequency: number) => void;
}

const RcSliderRadioTuner: React.FC<RadioTunerProps> = ({
  initialFrequency = 90.0,
  minFrequency = 88.0,
  maxFrequency = 108.0,
  onFrequencyChange,
}) => {
  const { state, dispatch } = useGameState();
  const audio = useAudio();

  // Use local state for everything
  const [frequency, setFrequency] = useState<number>(
    state.currentFrequency !== undefined ? state.currentFrequency : initialFrequency
  );
  const [signalStrength, setSignalStrength] = useState<number>(0);
  const [currentSignalId, setCurrentSignalId] = useState<string | null>(null);
  const [showMessage, setShowMessage] = useState<boolean>(false);
  const [staticIntensity, setStaticIntensity] = useState<number>(0.5);
  const [isScanning, setIsScanning] = useState<boolean>(false);

  const staticCanvasRef = useRef<HTMLCanvasElement>(null);
  const scanIntervalRef = useRef<number | null>(null);

  // Initialize frequency from game state on component mount
  useEffect(() => {
    if (state.currentFrequency !== undefined) {
      setFrequency(state.currentFrequency);
    } else {
      // If game state doesn't have a frequency yet, initialize it
      dispatch({ type: 'SET_FREQUENCY', payload: initialFrequency });
    }
  }, []); // Only run on mount

  // Handle frequency change from the slider
  const handleSliderChange = (value: number | number[]) => {
    if (typeof value === 'number') {
      const roundedFreq = parseFloat(value.toFixed(1));

      // Update local state
      setFrequency(roundedFreq);

      // Update game state directly
      dispatch({ type: 'SET_FREQUENCY', payload: roundedFreq });

      // Call the callback if provided
      if (onFrequencyChange) {
        onFrequencyChange(roundedFreq);
      }
    }
  };

  // Detect signals and update audio based on frequency
  useEffect(() => {
    // Only process if the radio is on
    if (!state.isRadioOn) {
      // Stop all audio when radio is off
      audio.stopSignal();
      audio.stopStaticNoise();
      return;
    }

    // Check if there's a signal at this frequency
    const signal = findSignalAtFrequency(frequency);

    if (signal) {
      // Calculate signal strength based on how close we are to the exact frequency
      const strength = calculateSignalStrength(frequency, signal);
      setSignalStrength(strength);

      // If this is a new signal discovery, add it to discovered frequencies
      if (!state.discoveredFrequencies.includes(signal.frequency)) {
        dispatch({ type: 'ADD_DISCOVERED_FREQUENCY', payload: signal.frequency });
      }

      // Set the current signal ID for message display
      setCurrentSignalId(signal.messageId);

      // Calculate static intensity based on signal strength
      const intensity = signal.isStatic ? 1 - strength : (1 - strength) * 0.5;
      setStaticIntensity(intensity);

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
      setStaticIntensity(intensity);
      setSignalStrength(0.1); // Low signal strength
      setCurrentSignalId(null);

      audio.stopSignal();
      audio.playStaticNoise(intensity);
    }

    // Clean up audio when component unmounts or frequency changes
    return () => {
      // We'll handle cleanup in the component unmount effect
    };
  }, [frequency, dispatch, audio, state.discoveredFrequencies, state.isRadioOn]);

  // Draw static visualization with performance optimizations
  useEffect(() => {
    if (!staticCanvasRef.current || !state.isRadioOn) return;

    const canvas = staticCanvasRef.current;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Set canvas dimensions only if they've changed
    if (canvas.width !== canvas.clientWidth || canvas.height !== canvas.clientHeight) {
      canvas.width = canvas.clientWidth;
      canvas.height = canvas.clientHeight;
    }

    // Animation loop for continuous static effect
    let animationId: number;
    let lastFrameTime = 0;
    const frameRate = 30; // Limit to 30 FPS for better performance
    const frameInterval = 1000 / frameRate;

    const animate = (timestamp: number): void => {
      if (!state.isRadioOn) {
        // If radio is turned off during animation, cancel the frame
        return;
      }

      const elapsed = timestamp - lastFrameTime;

      // Only render if enough time has passed (frame rate limiting)
      if (elapsed > frameInterval) {
        lastFrameTime = timestamp - (elapsed % frameInterval);

        // Clear canvas
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // Draw static noise with color variations based on signal strength
        const intensity = staticIntensity * 255;
        const signalColor = signalStrength > 0.5 ?
          `rgba(${100 + signalStrength * 155}, ${100 + signalStrength * 155}, 255, 0.5)` :
          'rgba(255, 255, 255, 0.5)';

        for (let i = 0; i < canvas.width; i += 2) {
          for (let j = 0; j < canvas.height; j += 2) {
            const noiseValue = Math.random() * intensity;
            const useSignalColor = signalStrength > 0.3 && Math.random() < signalStrength * 0.3;

            ctx.fillStyle = useSignalColor ? signalColor : `rgba(${noiseValue}, ${noiseValue}, ${noiseValue}, 0.5)`;
            ctx.fillRect(i, j, 2, 2);
          }
        }
      }

      animationId = requestAnimationFrame(animate);
    };

    animationId = requestAnimationFrame(animate);

    return () => {
      if (animationId) {
        cancelAnimationFrame(animationId);
      }
    };
  }, [staticIntensity, state.isRadioOn, signalStrength]);

  // Toggle message display
  const toggleMessage = (): void => {
    setShowMessage(!showMessage);
  };

  // Calculate dial position based on current frequency
  const dialPosition = ((frequency - minFrequency) / (maxFrequency - minFrequency)) * 100;

  // Toggle frequency scanning
  const toggleScanning = (): void => {
    if (isScanning) {
      // Stop scanning
      setIsScanning(false);
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    } else {
      // Start scanning
      setIsScanning(true);
      scanIntervalRef.current = window.setInterval(() => {
        // Increment frequency by 0.1 MHz
        const newFreq = frequency + 0.1;
        // If we reach the max frequency, loop back to min
        const nextFreq = newFreq > maxFrequency ? minFrequency : parseFloat(newFreq.toFixed(1));

        // Update both local state and game state
        setFrequency(nextFreq);
        dispatch({ type: 'SET_FREQUENCY', payload: nextFreq });

        // Call the callback if provided
        if (onFrequencyChange) {
          onFrequencyChange(nextFreq);
        }
      }, 300); // Scan speed in milliseconds
    }
  };

  // Clean up interval on unmount
  useEffect(() => {
    return () => {
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
      }
    };
  }, []);

  // Get the current message
  const currentMessage = currentSignalId ? getMessage(currentSignalId) : undefined;

  // Add a cleanup effect for when the component unmounts
  useEffect(() => {
    return () => {
      // Clean up all audio resources when component unmounts
      audio.stopSignal();
      audio.stopStaticNoise();

      // Clear any remaining intervals
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    };
  }, [audio]);

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
            onClick={() => dispatch({ type: 'TOGGLE_RADIO' })}
            aria-label={state.isRadioOn ? 'Turn radio off' : 'Turn radio on'}
          >
            {state.isRadioOn ? 'ON' : 'OFF'}
          </button>
        </div>

        <div
          className="frequency-display"
          aria-live="polite"
          aria-label={`Current frequency ${frequency.toFixed(1)} MHz`}
        >
          <span className="frequency-value">{frequency.toFixed(1)}</span>
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
          value={frequency}
          onChange={handleSliderChange}
          disabled={!state.isRadioOn || isScanning}
          marks={marks}
          railStyle={{ backgroundColor: '#333', height: 10 }}
          trackStyle={{ backgroundColor: '#666', height: 10 }}
          handleStyle={{
            borderColor: '#007bff',
            height: 28,
            width: 28,
            marginTop: -9,
            backgroundColor: '#fff',
            boxShadow: '0 0 5px rgba(0, 123, 255, 0.5)',
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
            const newFreq = Math.max(minFrequency, parseFloat((frequency - 0.1).toFixed(1)));
            setFrequency(newFreq);
            dispatch({ type: 'SET_FREQUENCY', payload: newFreq });
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
            const newFreq = Math.min(maxFrequency, parseFloat((frequency + 0.1).toFixed(1)));
            setFrequency(newFreq);
            dispatch({ type: 'SET_FREQUENCY', payload: newFreq });
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
            <MessageDisplay message={currentMessage} onClose={toggleMessage} />
          )}
        </div>
      )}
    </div>
  );
};

export default RcSliderRadioTuner;
