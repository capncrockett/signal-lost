import React, { useState, useEffect, useRef } from 'react';
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
  const { state, dispatch } = useGameState();
  const audio = useAudio();

  const [frequency, setFrequency] = useState<number>(initialFrequency);
  const [isDragging, setIsDragging] = useState<boolean>(false);
  const [signalStrength, setSignalStrength] = useState<number>(0);
  const [currentSignalId, setCurrentSignalId] = useState<string | null>(null);
  const [showMessage, setShowMessage] = useState<boolean>(false);
  const [staticIntensity, setStaticIntensity] = useState<number>(0.5);
  const [isScanning, setIsScanning] = useState<boolean>(false);
  const staticCanvasRef = useRef<HTMLCanvasElement>(null);
  const scanIntervalRef = useRef<number | null>(null);

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

  // Update the frequency in the game state
  useEffect(() => {
    dispatch({ type: 'SET_FREQUENCY', payload: frequency });

    if (onFrequencyChange) {
      onFrequencyChange(frequency);
    }
  }, [frequency, dispatch, onFrequencyChange]);

  // Detect signals and update audio based on frequency
  useEffect(() => {
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
        if (strength > 0.5) {
          audio.playSignal(signal.frequency * 10); // Scale up for audible range
        }
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
      // Don't stop audio here, as it would cause interruptions during tuning
      // We'll just update it in the next effect run
    };
  }, [frequency, dispatch, audio, state.discoveredFrequencies]);

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

  // Draw static visualization
  useEffect(() => {
    if (!staticCanvasRef.current || !state.isRadioOn) return;

    const canvas = staticCanvasRef.current;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Set canvas dimensions
    canvas.width = canvas.clientWidth;
    canvas.height = canvas.clientHeight;

    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Only draw static if radio is on
    if (state.isRadioOn) {
      // Draw static noise
      const intensity = staticIntensity * 255;
      const imageData = ctx.createImageData(canvas.width, canvas.height);
      const data = imageData.data;

      for (let i = 0; i < data.length; i += 4) {
        const noise = Math.random() * intensity;
        data[i] = noise; // R
        data[i + 1] = noise; // G
        data[i + 2] = noise; // B
        data[i + 3] = Math.random() * 255 * staticIntensity; // A
      }

      ctx.putImageData(imageData, 0, 0);
    }

    // Animation loop for continuous static effect
    let animationId: number;
    const animate = (): void => {
      if (!state.isRadioOn) return;

      // Clear canvas
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Draw static noise
      const intensity = staticIntensity * 255;
      const imageData = ctx.createImageData(canvas.width, canvas.height);
      const data = imageData.data;

      for (let i = 0; i < data.length; i += 4) {
        const noise = Math.random() * intensity;
        data[i] = noise; // R
        data[i + 1] = noise; // G
        data[i + 2] = noise; // B
        data[i + 3] = Math.random() * 255 * staticIntensity; // A
      }

      ctx.putImageData(imageData, 0, 0);
      animationId = requestAnimationFrame(animate);
    };

    animate();

    return () => {
      cancelAnimationFrame(animationId);
    };
  }, [staticIntensity, state.isRadioOn]);

  // Toggle message display
  const toggleMessage = (): void => {
    setShowMessage(!showMessage);
  };

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
        setFrequency((prev) => {
          const newFreq = prev + 0.1;
          // If we reach the max frequency, loop back to min
          return newFreq > maxFrequency ? minFrequency : newFreq;
        });
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

  // Handle keyboard controls for accessibility
  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (!state.isRadioOn) return;

    switch (e.key) {
      case 'ArrowLeft':
        if (!isScanning) {
          setFrequency((prev): number => Math.max(minFrequency, prev - 0.1));
        }
        break;
      case 'ArrowRight':
        if (!isScanning) {
          setFrequency((prev): number => Math.min(maxFrequency, prev + 0.1));
        }
        break;
      case 'ArrowDown':
        if (!isScanning) {
          setFrequency((prev): number => Math.max(minFrequency, prev - 1.0));
        }
        break;
      case 'ArrowUp':
        if (!isScanning) {
          setFrequency((prev): number => Math.min(maxFrequency, prev + 1.0));
        }
        break;
      case 's':
      case 'S':
        // Toggle scanning with 's' key
        toggleScanning();
        break;
      case 'Escape':
        // Stop scanning with Escape key
        if (isScanning) {
          toggleScanning();
        }
        break;
      default:
        break;
    }
  };

  return (
    <div
      className="radio-tuner"
      data-testid="radio-tuner"
      tabIndex={0}
      onKeyDown={handleKeyDown}
      aria-label="Radio Tuner. Use arrow keys to adjust frequency."
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
      </div>

      <div
        className={`tuner-dial-container ${!state.isRadioOn ? 'disabled' : ''}`}
        onMouseDown={state.isRadioOn ? handleMouseDown : undefined}
        onMouseUp={state.isRadioOn ? handleMouseUp : undefined}
        onMouseMove={state.isRadioOn ? handleMouseMove : undefined}
        role="slider"
        aria-valuemin={minFrequency}
        aria-valuemax={maxFrequency}
        aria-valuenow={frequency}
        aria-valuetext={`${frequency.toFixed(1)} MHz`}
        aria-label="Frequency dial"
      >
        <div className="tuner-dial-track">
          <div className="tuner-dial-knob" style={{ left: `${dialPosition}%` }} />
        </div>
      </div>

      <div className={`signal-strength-container ${!state.isRadioOn ? 'disabled' : ''}`}>
        <div className="signal-strength-label">Signal Strength</div>
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
        </div>
      </div>

      <div className="tuner-controls">
        <button
          className="tune-button decrease"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => setFrequency((prev) => Math.max(minFrequency, prev - 0.1))}
          aria-label="Decrease frequency by 0.1 MHz"
        >
          -0.1
        </button>
        <button
          className="tune-button increase"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => setFrequency((prev) => Math.min(maxFrequency, prev + 0.1))}
          aria-label="Increase frequency by 0.1 MHz"
        >
          +0.1
        </button>
        <button
          className={`scan-button ${isScanning ? 'scanning' : ''}`}
          disabled={!state.isRadioOn}
          onClick={() => toggleScanning()}
          aria-label={isScanning ? 'Stop scanning' : 'Start scanning for signals'}
        >
          {isScanning ? 'Stop Scan' : 'Scan'}
        </button>
      </div>

      {state.isRadioOn && currentSignalId && signalStrength > 0.5 && (
        <div className="message-indicator" aria-live="polite">
          <div className="signal-detected">Signal Detected</div>
          <button
            className="view-message-button"
            onClick={toggleMessage}
            aria-label={showMessage ? 'Hide message content' : 'View message content'}
          >
            {showMessage ? 'Hide Message' : 'View Message'}
          </button>
        </div>
      )}

      {currentMessage && (
        <MessageDisplay
          message={currentMessage}
          isVisible={showMessage && state.isRadioOn && signalStrength > 0.5}
        />
      )}
    </div>
  );
};

export default RadioTuner;
