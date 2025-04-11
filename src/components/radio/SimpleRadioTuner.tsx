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

const SimpleRadioTuner: React.FC<RadioTunerProps> = ({
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
  const [isDragging, setIsDragging] = useState<boolean>(false);
  const [signalStrength, setSignalStrength] = useState<number>(0);
  const [currentSignalId, setCurrentSignalId] = useState<string | null>(null);
  const [showMessage, setShowMessage] = useState<boolean>(false);
  const [staticIntensity, setStaticIntensity] = useState<number>(0.5);
  const [isScanning, setIsScanning] = useState<boolean>(false);
  
  const staticCanvasRef = useRef<HTMLCanvasElement>(null);
  const scanIntervalRef = useRef<number | null>(null);

  // Sync local frequency with game state
  useEffect(() => {
    if (state.currentFrequency !== undefined && state.currentFrequency !== frequency) {
      setFrequency(state.currentFrequency);
    }
  }, [state.currentFrequency, frequency]);

  // Update game state when local frequency changes
  useEffect(() => {
    dispatch({ type: 'SET_FREQUENCY', payload: frequency });
    
    if (onFrequencyChange) {
      onFrequencyChange(frequency);
    }
  }, [frequency, dispatch, onFrequencyChange]);

  // Handle mouse down on the dial
  const handleMouseDown = (): void => {
    setIsDragging(true);
  };

  // Handle mouse up
  const handleMouseUp = (): void => {
    setIsDragging(false);
  };

  // Handle mouse move on the dial
  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>): void => {
    if (!isDragging || !state.isRadioOn) return;

    const container = e.currentTarget;
    const rect = container.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const width = rect.width;
    const percentage = Math.max(0, Math.min(1, x / width));

    const newFrequency = minFrequency + percentage * (maxFrequency - minFrequency);
    setFrequency(parseFloat(newFrequency.toFixed(1)));
  };

  // Add global mouse event listeners for dragging
  useEffect(() => {
    const handleGlobalMouseUp = (): void => {
      setIsDragging(false);
    };

    const handleGlobalMouseMove = (e: MouseEvent): void => {
      if (!isDragging || !state.isRadioOn) return;
      
      // Get the dial container element
      const dialContainer = document.querySelector('.tuner-dial-container');
      if (!dialContainer) return;
      
      const rect = dialContainer.getBoundingClientRect();
      const x = e.clientX - rect.left;
      const width = rect.width;
      const percentage = Math.max(0, Math.min(1, x / width));
      
      const newFrequency = minFrequency + percentage * (maxFrequency - minFrequency);
      setFrequency(parseFloat(newFrequency.toFixed(1)));
    };

    if (isDragging) {
      window.addEventListener('mouseup', handleGlobalMouseUp);
      window.addEventListener('mousemove', handleGlobalMouseMove);
    }

    return () => {
      window.removeEventListener('mouseup', handleGlobalMouseUp);
      window.removeEventListener('mousemove', handleGlobalMouseMove);
    };
  }, [isDragging, state.isRadioOn, minFrequency, maxFrequency]);

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

  // Calculate dial position based on current frequency
  const dialPosition = ((frequency - minFrequency) / (maxFrequency - minFrequency)) * 100;

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
          return newFreq > maxFrequency ? minFrequency : parseFloat(newFreq.toFixed(1));
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

  // Handle keyboard controls for accessibility
  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (!state.isRadioOn) return;

    switch (e.key) {
      case 'ArrowLeft':
        if (!isScanning) {
          setFrequency((prev) => Math.max(minFrequency, prev - 0.1));
        }
        break;
      case 'ArrowRight':
        if (!isScanning) {
          setFrequency((prev) => Math.min(maxFrequency, prev + 0.1));
        }
        break;
      case 'ArrowDown':
        if (!isScanning) {
          setFrequency((prev) => Math.max(minFrequency, prev - 1.0));
        }
        break;
      case 'ArrowUp':
        if (!isScanning) {
          setFrequency((prev) => Math.min(maxFrequency, prev + 1.0));
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
        <div
          className="static-overlay"
          style={{
            opacity: state.isRadioOn ? 0.7 : 0,
          }}
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
        <div className="frequency-markers">
          {Array.from({ length: 11 }, (_, i) => {
            const markerFrequency = minFrequency + (i * (maxFrequency - minFrequency)) / 10;
            const isMajor = i % 2 === 0;
            return (
              <div
                key={i}
                className={`frequency-marker ${isMajor ? 'major' : ''}`}
                style={{ left: `${i * 10}%` }}
              >
                {isMajor && markerFrequency.toFixed(1)}
              </div>
            );
          })}
        </div>
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

export default SimpleRadioTuner;
