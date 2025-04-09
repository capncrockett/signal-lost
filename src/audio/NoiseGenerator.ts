import * as Tone from 'tone';
import { NoiseType } from './NoiseType';

/**
 * Result of creating a noise generator
 */
export interface NoiseResult {
  noise: Tone.Noise;
  gain: Tone.Gain<'gain'>;
  filter?: Tone.Filter;
}

/**
 * Noise generator configuration options
 */
export interface NoiseOptions {
  type?: NoiseType;
  volume?: number;
  filterType?: string;
  filterFrequency?: number;
  filterQ?: number;
  applyFilter?: boolean;
}

/**
 * Default noise options
 */
const defaultNoiseOptions: NoiseOptions = {
  type: NoiseType.Pink,
  volume: 0.1125, // Half of the original 0.225
  filterType: 'lowpass',
  filterFrequency: 1000,
  filterQ: 1,
  applyFilter: true,
};

/**
 * Create a noise generator with the specified options
 * @param options Configuration options for the noise generator
 * @returns The noise generator and associated audio nodes, or null if creation failed
 */
export function createNoise(options?: NoiseOptions): NoiseResult | null {
  try {
    // Merge provided options with defaults
    const config: NoiseOptions = { ...defaultNoiseOptions, ...options };

    // Create a noise generator
    const noise = new Tone.Noise(config.type).start();

    // Create a gain node to control the volume
    const gain = new Tone.Gain(config.volume);

    // Create a filter if requested
    let filter: Tone.Filter | undefined;

    if (config.applyFilter) {
      filter = new Tone.Filter({
        type: config.filterType,
        frequency: config.filterFrequency,
        Q: config.filterQ,
      } as Tone.FilterOptions);

      // Connect the noise to the filter to the gain to the output
      noise.connect(filter);
      filter.connect(gain);
    } else {
      // Connect the noise directly to the gain
      noise.connect(gain);
    }

    // Connect the gain to the destination
    gain.toDestination();

    return { noise, gain, filter };
  } catch (error) {
    console.error('Failed to create noise generator:', error);
    return null;
  }
}

/**
 * Create a signal generator at the specified frequency
 * @param frequency The frequency of the signal in Hz
 * @param volume The volume of the signal (default: 0.5)
 * @param waveform The waveform type (default: sine)
 * @returns The oscillator and gain node, or null if creation failed
 */
export function createSignal(
  frequency: number,
  volume: number = 0.5,
  waveform: Tone.ToneOscillatorType = 'sine'
): { oscillator: Tone.Oscillator; gain: Tone.Gain<'gain'> } | null {
  try {
    // Create an oscillator
    const oscillator = new Tone.Oscillator(frequency, waveform).start();

    // Create a gain node to control the volume
    const gain = new Tone.Gain(volume).toDestination();

    // Connect the oscillator to the gain node
    oscillator.connect(gain);

    return { oscillator, gain };
  } catch (error) {
    console.error('Failed to create signal generator:', error);
    return null;
  }
}
