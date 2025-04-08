import React, { useState, useEffect } from 'react';
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

      // Play appropriate audio
      if (signal.isStatic) {
        // Play static with the signal mixed in
        audio.playStaticNoise(1 - strength);
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
      const staticIntensity = getStaticIntensity(frequency);
      setSignalStrength(0.1); // Low signal strength
      setCurrentSignalId(null);

      audio.stopSignal();
      audio.playStaticNoise(staticIntensity);
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

  // Toggle message display
  const toggleMessage = (): void => {
    setShowMessage(!showMessage);
  };

  // Get the current message
  const currentMessage = currentSignalId ? getMessage(currentSignalId) : undefined;

  return (
    <div className="radio-tuner" data-testid="radio-tuner">
      <div className="radio-controls">
        <div className="power-button-container">
          <button
            className={`power-button ${state.isRadioOn ? 'on' : 'off'}`}
            onClick={() => dispatch({ type: 'TOGGLE_RADIO' })}
          >
            {state.isRadioOn ? 'ON' : 'OFF'}
          </button>
        </div>

        <div className="frequency-display">
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
          />
          <button
            className={`mute-button ${audio.isMuted ? 'muted' : ''}`}
            onClick={audio.toggleMute}
          >
            {audio.isMuted ? 'Unmute' : 'Mute'}
          </button>
        </div>
      </div>

      <div
        className={`tuner-dial-container ${!state.isRadioOn ? 'disabled' : ''}`}
        onMouseDown={state.isRadioOn ? handleMouseDown : undefined}
        onMouseUp={state.isRadioOn ? handleMouseUp : undefined}
        onMouseMove={state.isRadioOn ? handleMouseMove : undefined}
      >
        <div className="tuner-dial-track">
          <div className="tuner-dial-knob" style={{ left: `${dialPosition}%` }} />
        </div>
      </div>

      <div className={`signal-strength-container ${!state.isRadioOn ? 'disabled' : ''}`}>
        <div className="signal-strength-label">Signal Strength</div>
        <div className="signal-strength-meter">
          <div
            className="signal-strength-fill"
            style={{ width: `${state.isRadioOn ? signalStrength * 100 : 0}%` }}
          />
        </div>
      </div>

      <div className="tuner-controls">
        <button
          className="tune-button decrease"
          disabled={!state.isRadioOn}
          onClick={() => setFrequency((prev) => Math.max(minFrequency, prev - 0.1))}
        >
          -0.1
        </button>
        <button
          className="tune-button increase"
          disabled={!state.isRadioOn}
          onClick={() => setFrequency((prev) => Math.min(maxFrequency, prev + 0.1))}
        >
          +0.1
        </button>
      </div>

      {state.isRadioOn && currentSignalId && signalStrength > 0.5 && (
        <div className="message-indicator">
          <div className="signal-detected">Signal Detected</div>
          <button className="view-message-button" onClick={toggleMessage}>
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
