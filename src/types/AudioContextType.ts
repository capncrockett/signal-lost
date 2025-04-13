import * as Tone from 'tone';
import { NoiseOptions } from '../audio/NoiseGenerator';
import { NoiseType } from '../audio/NoiseType';

export interface AudioContextType {
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
