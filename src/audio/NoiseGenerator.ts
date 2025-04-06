import * as Tone from 'tone';
import { NoiseType } from './NoiseType';

/**
 * Result of creating a noise generator
 */
export interface NoiseResult {
  noise: Tone.Noise;
  gain: Tone.Gain<'gain'>;
}

/**
 * Create a noise generator with the specified type and volume
 * @param type The type of noise to generate (default: pink)
 * @param volume The volume of the noise (default: 0.1125 - half of the original 0.225)
 * @returns The noise generator and gain node, or null if creation failed
 */
export function createNoise(
  type: NoiseType = NoiseType.Pink,
  volume: number = 0.1125 // Half of the original 0.225
): NoiseResult | null {
  try {
    // Create a noise generator
    const noise = new Tone.Noise(type).start();

    // Create a gain node to control the volume
    const gain = new Tone.Gain(volume).toDestination();

    // Connect the noise to the gain node
    noise.connect(gain);

    return { noise, gain };
  } catch (error) {
    console.error('Failed to create noise generator:', error);
    return null;
  }
}
