import React, { useState, useEffect } from 'react';
import { useAudio } from '../../src/context/AudioContext';
import { useGameState } from '../../src/context/GameStateContext';
import {
  findSignalAtFrequency,
  calculateSignalStrength,
  getStaticIntensity,
} from '../../src/data/frequencies';
import { getMessage } from '../../src/data/messages';
import { NoiseType } from '../../src/audio/NoiseType';
import { createNoise, createSignal } from '../../src/audio/NoiseGenerator';

interface RadioTunerProps {
  initialFrequency?: number;
  minFrequency?: number;
  maxFrequency?: number;
  onFrequencyChange?: (frequency: number) => void;
}

// This is a simplified version of RadioTuner for testing
const RadioTunerMock: React.FC<RadioTunerProps> = ({
  initialFrequency = 90.0,
  minFrequency = 88.0,
  maxFrequency = 108.0,
  // We're not using this prop in the mock, but it's part of the interface
  // onFrequencyChange,
}) => {
  const { state, dispatch } = useGameState();
  const audio = useAudio();

  const [frequency, setFrequency] = useState<number>(initialFrequency);
  // We're not using this state in the mock, but it's part of the component logic
  // const [signalStrength, setSignalStrength] = useState<number>(0);
  const [currentSignalId, setCurrentSignalId] = useState<string | null>(null);
  const [isScanning, setIsScanning] = useState<boolean>(false);

  // Update frequency and handle audio changes
  useEffect(() => {
    if (!state.isRadioOn) return;

    dispatch({ type: 'SET_FREQUENCY', payload: frequency });

    // Check for signal
    const signal = findSignalAtFrequency(frequency);

    if (signal) {
      const strength = calculateSignalStrength(frequency, signal);
      setSignalStrength(strength);
      setCurrentSignalId(signal.messageId);

      if (signal.isStatic) {
        // Create static noise
        createNoise({
          type: audio.currentNoiseType,
          volume: 1 - strength,
        });

        if (strength > 0.5) {
          // Create signal
          createSignal(signal.frequency * 10);
        }
      } else {
        // Create only signal for clear frequencies
        createSignal(signal.frequency * 10);
      }
    } else {
      const intensity = getStaticIntensity(frequency);
      setSignalStrength(0.1);
      setCurrentSignalId(null);

      // Create only static noise for frequencies without signals
      createNoise({
        type: audio.currentNoiseType,
        volume: intensity,
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [frequency, state.isRadioOn, audio.currentNoiseType]);

  // Update frequency
  const updateFrequency = (newFrequency: number) => {
    const clampedFrequency = Math.max(minFrequency, Math.min(maxFrequency, newFrequency));
    setFrequency(clampedFrequency);
  };

  // Toggle radio power
  const togglePower = () => {
    dispatch({ type: 'TOGGLE_RADIO' });
  };

  // Toggle scanning
  const toggleScanning = () => {
    setIsScanning(!isScanning);
  };

  // Get current message
  // We're not using this in the mock, but it's part of the component logic
  // const currentMessage = currentSignalId ? getMessage(currentSignalId) : undefined;

  return (
    <div className="radio-tuner" data-testid="radio-tuner">
      <div className="radio-controls">
        <button className={`power-button ${state.isRadioOn ? 'on' : 'off'}`} onClick={togglePower}>
          {state.isRadioOn ? 'ON' : 'OFF'}
        </button>

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
            aria-label="Volume"
          />
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

      <div className="tuner-controls">
        <button
          className="tune-button decrease"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => updateFrequency(frequency - 0.1)}
        >
          -0.1
        </button>
        <button
          className="tune-button increase"
          disabled={!state.isRadioOn || isScanning}
          onClick={() => updateFrequency(frequency + 0.1)}
        >
          +0.1
        </button>
        <button
          className={`scan-button ${isScanning ? 'scanning' : ''}`}
          disabled={!state.isRadioOn}
          onClick={toggleScanning}
        >
          {isScanning ? 'Stop Scan' : 'Scan'}
        </button>
      </div>
    </div>
  );
};

export default RadioTunerMock;
