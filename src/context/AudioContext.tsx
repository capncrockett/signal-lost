import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import * as Tone from 'tone';

interface AudioContextType {
  isMuted: boolean;
  volume: number;
  toggleMute: () => void;
  setVolume: (volume: number) => void;
  playStaticNoise: (intensity: number) => void;
  stopStaticNoise: () => void;
  playSignal: (frequency: number) => void;
  stopSignal: () => void;
}

const AudioContext = createContext<AudioContextType | undefined>(undefined);

interface AudioProviderProps {
  children: ReactNode;
}

export const AudioProvider: React.FC<AudioProviderProps> = ({ children }) => {
  const [isMuted, setIsMuted] = useState<boolean>(false);
  const [volume, setVolumeState] = useState<number>(0.5);
  const [noiseNode, setNoiseNode] = useState<Tone.Noise | null>(null);
  const [oscillator, setOscillator] = useState<Tone.Oscillator | null>(null);
  const [filter, setFilter] = useState<Tone.Filter | null>(null);

  // Initialize audio components
  useEffect(() => {
    // Create a filter for the noise
    const newFilter = new Tone.Filter({
      type: 'lowpass',
      frequency: 1000,
      Q: 1,
    }).toDestination();

    setFilter(newFilter);

    // Clean up on unmount
    return () => {
      if (noiseNode) {
        noiseNode.stop();
        noiseNode.dispose();
      }
      if (oscillator) {
        oscillator.stop();
        oscillator.dispose();
      }
      if (newFilter) {
        newFilter.dispose();
      }
    };
  }, [noiseNode, oscillator]);

  // Update master volume when volume state changes
  useEffect(() => {
    Tone.Destination.volume.value = isMuted ? -Infinity : Tone.gainToDb(volume);
  }, [volume, isMuted]);

  const toggleMute = (): void => {
    setIsMuted(!isMuted);
  };

  const setVolume = (newVolume: number): void => {
    setVolumeState(Math.max(0, Math.min(1, newVolume)));
  };

  const playStaticNoise = (intensity: number): void => {
    if (!filter) return;

    // Stop any existing noise
    stopStaticNoise();

    // Create a new noise node
    const noise = new Tone.Noise({
      type: 'pink', // pink noise is less harsh than white noise
      volume: Tone.gainToDb(intensity * 0.5), // Reduce volume by half
    }).connect(filter);

    // Start the noise
    noise.start();
    setNoiseNode(noise);
  };

  const stopStaticNoise = (): void => {
    if (noiseNode) {
      noiseNode.stop();
      noiseNode.dispose();
      setNoiseNode(null);
    }
  };

  const playSignal = (frequency: number): void => {
    if (!filter) return;

    // Stop any existing oscillator
    stopSignal();

    // Create a new oscillator
    const osc = new Tone.Oscillator({
      frequency,
      type: 'sine',
      volume: -20, // Lower volume for the signal
    }).connect(filter);

    // Start the oscillator
    osc.start();
    setOscillator(osc);
  };

  const stopSignal = (): void => {
    if (oscillator) {
      oscillator.stop();
      oscillator.dispose();
      setOscillator(null);
    }
  };

  return (
    <AudioContext.Provider
      value={{
        isMuted,
        volume,
        toggleMute,
        setVolume,
        playStaticNoise,
        stopStaticNoise,
        playSignal,
        stopSignal,
      }}
    >
      {children}
    </AudioContext.Provider>
  );
};

// Custom hook for using the audio context
export const useAudio = (): AudioContextType => {
  const context = useContext(AudioContext);
  if (context === undefined) {
    throw new Error('useAudio must be used within an AudioProvider');
  }
  return context;
};

export default AudioContext;
