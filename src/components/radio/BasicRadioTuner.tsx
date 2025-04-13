import React, { useEffect, useRef, useState } from 'react';
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
  // For debugging
  console.log('BasicRadioTuner rendering', new Date().toISOString());
  
  // Context
  const { state, dispatch } = useGameState();
  const audio = useAudio();
  
  // Refs that don't trigger re-renders
  const staticCanvasRef = useRef<HTMLCanvasElement>(null);
  const scanIntervalRef = useRef<number | null>(null);
  const animationFrameRef = useRef<number | null>(null);
  
  // State that triggers re-renders when changed
  const [showMessage, setShowMessage] = useState(false);
  const [isScanning, setIsScanning] = useState(false);
  const [signalStrength, setSignalStrength] = useState(0);
  const [staticIntensity, setStaticIntensity] = useState(0.5);
  const [currentSignalId, setCurrentSignalId] = useState<string | null>(null);
  
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
    
    // Check if there's a signal at this frequency
    const signal = findSignalAtFrequency(state.currentFrequency);
    
    if (signal) {
      // Calculate signal strength based on how close we are to the exact frequency
      const strength = calculateSignalStrength(state.currentFrequency, signal);
      
      // Calculate static intensity based on signal strength
      const intensity = signal.isStatic ? 1 - strength : (1 - strength) * 0.5;
      
      // Update state
      setSignalStrength(strength);
      setCurrentSignalId(signal.messageId);
      setStaticIntensity(intensity);
      
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
      const intensity = getStaticIntensity(state.currentFrequency);
      
      // Update state
      setStaticIntensity(intensity);
      setSignalStrength(0.1); // Low signal strength
      setCurrentSignalId(null);
      
      audio.stopSignal();
      audio.playStaticNoise(intensity);
    }
  }, [state.isRadioOn, state.currentFrequency, state.discoveredFrequencies, audio, dispatch]);
  
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
    
    const drawStatic = () => {
      if (!isActive || !state.isRadioOn) return;
      
      // Clear canvas
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      
      // Draw static noise
      const intensity = staticIntensity * 255;
      
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
  }, [state.isRadioOn, staticIntensity]);
  
  // Handle scanning
  useEffect(() => {
    if (isScanning && state.isRadioOn) {
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
    } else {
      // Stop scanning
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    }
    
    // Cleanup
    return () => {
      if (scanIntervalRef.current !== null) {
        window.clearInterval(scanIntervalRef.current);
        scanIntervalRef.current = null;
      }
    };
  }, [isScanning, state.isRadioOn, state.currentFrequency, dispatch, maxFrequency, minFrequency, onFrequencyChange]);
  
  // Toggle message display
  const toggleMessage = () => {
    setShowMessage(prev => !prev);
  };
  
  // Toggle frequency scanning
  const toggleScanning = () => {
    setIsScanning(prev => !prev);
  };
  
  // Change frequency by a specific amount
  const changeFrequency = (amount: number) => {
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
  };
  
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
};

export default BasicRadioTuner;
