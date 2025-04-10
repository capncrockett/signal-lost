import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import * as Tone from 'tone';
import { createNoise, createSignal, NoiseOptions } from '../audio/NoiseGenerator';
import { NoiseType } from '../audio/NoiseType';

interface AudioContextType {
  isMuted: boolean;
  volume: number;
  toggleMute: () => void;
  setVolume: (volume: number) => void;
  playStaticNoise: (intensity: number, options?: NoiseOptions) => void;
  stopStaticNoise: () => void;
  playSignal: (frequency: number, volume?: number, waveform?: Tone.ToneOscillatorType) => void;
  stopSignal: () => void;
  setNoiseType: (type: NoiseType) => void;
  currentNoiseType: NoiseType;
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
  // We need to track the filter state even though it's not directly used in the component
  // It's used in the playStaticNoise function when a new filter is created
  const [, setFilter] = useState<Tone.Filter | null>(null);
  const [noiseType, setNoiseType] = useState<NoiseType>(NoiseType.Pink);
  const [noiseGain, setNoiseGain] = useState<Tone.Gain<'gain'> | null>(null);
  const [signalGain, setSignalGain] = useState<Tone.Gain<'gain'> | null>(null);

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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Empty dependency array to run only on mount/unmount

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

  const playStaticNoise = (intensity: number, options?: NoiseOptions): void => {
    // Stop any existing noise
    stopStaticNoise();

    // Create noise options with current noise type and intensity-based volume
    const noiseOptions: NoiseOptions = {
      type: noiseType,
      volume: intensity * 0.5, // Reduce volume by half
      filterType: 'lowpass',
      filterFrequency: 1000 + (1 - intensity) * 2000, // Adjust filter based on intensity
      filterQ: 1 + intensity, // Adjust resonance based on intensity
      applyFilter: true,
      ...options, // Allow overriding with provided options
    };

    // Create a new noise generator
    const result = createNoise(noiseOptions);
    if (result) {
      setNoiseNode(result.noise);
      setNoiseGain(result.gain);
      if (result.filter) {
        setFilter(result.filter);
      }
    }
  };

  const stopStaticNoise = (): void => {
    if (noiseNode) {
      noiseNode.stop();
      noiseNode.dispose();
      setNoiseNode(null);
    }

    if (noiseGain) {
      noiseGain.dispose();
      setNoiseGain(null);
    }
  };

  const playSignal = (
    frequency: number,
    signalVolume: number = 0.5,
    waveform: Tone.ToneOscillatorType = 'sine'
  ): void => {
    // Stop any existing oscillator
    stopSignal();

    // Create a new signal generator
    const result = createSignal(frequency, signalVolume, waveform);
    if (result) {
      setOscillator(result.oscillator);
      setSignalGain(result.gain);
    }
  };

  const stopSignal = (): void => {
    if (oscillator) {
      oscillator.stop();
      oscillator.dispose();
      setOscillator(null);
    }

    if (signalGain) {
      signalGain.dispose();
      setSignalGain(null);
    }
  };

  // Set the noise type
  const setNoiseTypeHandler = (type: NoiseType): void => {
    setNoiseType(type);

    // If noise is currently playing, update it with the new type
    if (noiseNode) {
      const currentVolume = noiseGain ? noiseGain.gain.value : 0.5;
      stopStaticNoise();
      playStaticNoise(currentVolume * 2, { type }); // Multiply by 2 to compensate for the 0.5 reduction
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
        setNoiseType: setNoiseTypeHandler,
        currentNoiseType: noiseType,
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
