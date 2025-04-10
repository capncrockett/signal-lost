import React, { createContext, useContext, useState, ReactNode } from 'react';
import { NoiseType } from '../../src/audio/NoiseType';
import { NoiseOptions } from '../../src/audio/NoiseGenerator';
import * as Tone from 'tone';

// Create a mock version of the AudioContext
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
  const [noiseType, setNoiseType] = useState<NoiseType>(NoiseType.Pink);

  const toggleMute = (): void => {
    setIsMuted(!isMuted);
  };

  const setVolume = (newVolume: number): void => {
    setVolumeState(Math.max(0, Math.min(1, newVolume)));
  };

  const playStaticNoise = jest.fn();
  const stopStaticNoise = jest.fn();
  const playSignal = jest.fn();
  const stopSignal = jest.fn();
  const setNoiseTypeHandler = (type: NoiseType): void => {
    setNoiseType(type);
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
