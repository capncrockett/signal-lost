import { findSignalAtFrequency, calculateSignalStrength, getStaticIntensity } from '../data/frequencies';
import { useRadioStore } from '../store/radioStore';
import { AudioContextType } from '../types/AudioContextType';

// Helper function to process frequency changes
export const processFrequency = (
  frequency: number,
  isRadioOn: boolean,
  audio: AudioContextType,
  discoveredFrequencies: number[],
  addDiscoveredFrequency: (freq: number) => void
) => {
  const {
    setSignalStrength,
    setStaticIntensity,
    setCurrentSignalId
  } = useRadioStore.getState();

  // Only process if the radio is on
  if (!isRadioOn) {
    audio.stopSignal();
    audio.stopStaticNoise();
    return;
  }

  // Check if there's a signal at this frequency
  const signal = findSignalAtFrequency(frequency);

  if (signal) {
    // Calculate signal strength based on how close we are to the exact frequency
    const strength = calculateSignalStrength(frequency, signal);

    // Calculate static intensity based on signal strength
    const intensity = signal.isStatic ? 1 - strength : (1 - strength) * 0.5;

    // Update state
    setSignalStrength(strength);
    setCurrentSignalId(signal.messageId);
    setStaticIntensity(intensity);

    // If this is a new signal discovery, add it to discovered frequencies
    if (!discoveredFrequencies.includes(signal.frequency)) {
      addDiscoveredFrequency(signal.frequency);
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
    const intensity = getStaticIntensity(frequency);

    // Update state
    setStaticIntensity(intensity);
    setSignalStrength(0.1); // Low signal strength
    setCurrentSignalId(null);

    audio.stopSignal();
    audio.playStaticNoise(intensity);
  }
};
