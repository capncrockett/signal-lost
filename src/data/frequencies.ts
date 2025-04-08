// Define the valid signal frequencies and their associated messages
export interface SignalFrequency {
  frequency: number;
  tolerance: number; // How close the tuner needs to be to detect the signal
  signalStrength: number; // Base signal strength (0-1)
  messageId: string; // ID of the message to display when this frequency is tuned
  isStatic: boolean; // Whether this frequency produces static or a clear signal
  discovered?: boolean; // Whether the player has discovered this frequency
}

// Define the frequencies that contain signals
export const signalFrequencies: SignalFrequency[] = [
  {
    frequency: 91.1,
    tolerance: 0.1,
    signalStrength: 0.8,
    messageId: 'intro_signal',
    isStatic: false,
  },
  {
    frequency: 94.7,
    tolerance: 0.2,
    signalStrength: 0.6,
    messageId: 'distress_call',
    isStatic: true,
  },
  {
    frequency: 97.3,
    tolerance: 0.1,
    signalStrength: 0.9,
    messageId: 'coordinates',
    isStatic: false,
  },
  {
    frequency: 103.5,
    tolerance: 0.3,
    signalStrength: 0.5,
    messageId: 'warning',
    isStatic: true,
  },
  {
    frequency: 106.2,
    tolerance: 0.1,
    signalStrength: 0.7,
    messageId: 'final_message',
    isStatic: false,
  },
];

// Function to check if a frequency is close to a signal
export const findSignalAtFrequency = (frequency: number): SignalFrequency | null => {
  for (const signal of signalFrequencies) {
    if (Math.abs(frequency - signal.frequency) <= signal.tolerance) {
      return signal;
    }
  }
  return null;
};

// Function to calculate signal strength based on how close the frequency is to the signal
export const calculateSignalStrength = (frequency: number, signal: SignalFrequency): number => {
  const distance = Math.abs(frequency - signal.frequency);
  const normalizedDistance = distance / signal.tolerance;

  // Signal strength decreases as distance from exact frequency increases
  const strength = signal.signalStrength * (1 - normalizedDistance);

  // Add some randomness to make it feel more realistic
  const noise = Math.random() * 0.1 - 0.05; // -0.05 to 0.05

  return Math.max(0, Math.min(1, strength + noise));
};

// Function to get static intensity based on frequency
export const getStaticIntensity = (frequency: number): number => {
  // Check if we're near a signal
  const signal = findSignalAtFrequency(frequency);

  if (signal) {
    // If we're near a signal, static depends on the signal type and distance
    if (signal.isStatic) {
      // Static signal has more noise
      return 0.3 + (1 - calculateSignalStrength(frequency, signal)) * 0.7;
    } else {
      // Clear signal has less noise
      return (1 - calculateSignalStrength(frequency, signal)) * 0.5;
    }
  }

  // If we're not near any signal, generate random static based on frequency
  // Some frequency ranges have more static than others
  const baseStatic = 0.3;
  const frequencyFactor = Math.sin(frequency * 0.5) * 0.2 + 0.2;

  return baseStatic + frequencyFactor;
};
